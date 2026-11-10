using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Armors;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Level.Progression;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using _GAME.Scripts.Battle.Player.SecondaryWeapon;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Map;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _GAME.Scripts.Battle.Player
{
    [Serializable]
    public class ArmorRendererLink
    {
        // Inventory id of the armor item (e.g. ArmorDefense_Rare)
        public string InventoryId;
        public List<SkinnedMeshRenderer> Renderers = new();
    }

    public class Player: DamageReceiver, IDamageDealer
    {
        [SerializeField] private PlayerData _data;
        [SerializeField] private PlayerViewer _viewer;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private Transform _weaponRoot;
        [SerializeField] private WeaponType _weaponType;
        [SerializeField] private ArmorType _armorType;
        [Tooltip("Armor as equipment is temporarily disabled: no armor stats, visuals untouched")]
        [SerializeField] private bool _useArmor = false;
        [Tooltip("Energy shield is temporarily disabled")]
        [SerializeField] private bool _useEnergyShield = false;
        [SerializeField] private SecondaryWeaponType _secondaryWeaponType;
        [SerializeField] private SecondaryWeaponController _secondaryWeaponController;
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private Transform _freeWeaponHolder;
        [SerializeField] private RigBuilder _rig;
        [SerializeField] private StateIntAttribute _health;
        [SerializeField] private StateIntAttribute _energyShield;
        
        [SerializeField, ReadOnly]
        private DamageRepeater[] _damageRepeaters;
        [SerializeField] private DamageReactionViewer _damageReactionViewer;
        [Header("Armor visuals")]
        [SerializeField] private List<ArmorRendererLink> _armorRenderers = new();
        [SerializeField]
        private Weapon _weapon;
        private ArmorData _armorData;
        private CameraController _cameraController;
        private GameInput _gameInput;
        private bool _battleReady = false;
        private bool _previousFireState = false;

        private bool _isAimLock = false;
        
        public override Team Team => Team.Player;
        public bool IsDead { get; private set; }
        public StateIntAttribute Health => _health;
        public StateIntAttribute EnergyShield => _energyShield;
        public ArmorData ArmorData => _armorData;
        // Defense is a real percent of incoming health damage blocked (90 defense = -90% damage)
        private float DefenseFactor => _armorData != null ? Mathf.Clamp01(_armorData.Defense / 100f) : 0f;

        public bool IsReadySecondaryWeapon => _secondaryWeaponController != null && _secondaryWeaponController.IsReady;
        public float SecondaryWeaponProgress => _secondaryWeaponController != null? _secondaryWeaponController.Progress: 0f;
        
        public event Action OnDestinationTargetPosition;
        public event Action<bool> OnBattleReady;
        private Action OnBattleStop;
        public event Action OnHit;
        public event Action OnShot;
        
        public event Action OnSeccondaryShot;

        public event Action OnDamaged;

        private void Awake()
        {
            _health.OnChangeValue += OnHealthChange;
            _energyShield.OnChangeValue += OnEnergyShieldChange;
        }

        public void Setup(CameraController cameraController)
        {
            _cameraController = cameraController;
            _gameInput = new GameInput();
            
            SetupWeapon();
            SetupArmor();
            _aim.Setup(_gameInput, _weapon);
            _secondaryWeaponController.Setup(this, _aim, GetSecondaryWeaponData());
            _health.Set(_data.Health + GetProgressHealthBonus() + (_armorData?.Health ?? 0));
            _energyShield.Set(_useEnergyShield ? _armorData?.EnergyShield ?? 0 : 0);
            
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
            }
            
            EventBus.Subscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
        }

        private void SetupWeapon()
        {
            _rig.enabled = false;
            var weapons = Core.Get<DataBase>().Weapons;
            WeaponData weaponData = null;
            if (TryGetEquipped(InventoryCase.Weapon, out var weaponId, out var weaponLevel))
            {
                weaponData = weapons.GetData(weaponId);
            }
            if (weaponData == null)
            {
                weaponData = weapons.GetData(_weaponType);
                weaponLevel = _data.WeaponLevel;
            }
            weaponData.Setup(this, weaponLevel);
            if (_weapon != null)
            {
                Weapon.Release(_weapon);
            }
            _weapon = Weapon.Create(weaponData.Type);
            _weapon.transform.SetParent(_weaponRoot);
            _weapon.transform.localPosition = Vector3.zero;
            _weapon.transform.localRotation = Quaternion.identity;
            _weapon.gameObject.SetActive(true);
            _weapon.Setup(weaponData, IsFire, OnHitHandler, _freeWeaponHolder, OnFire);
            OnHit += _secondaryWeaponController.OnHit;
            OnShot += _viewer.Fire;
            _rig.enabled = true;
        }

        private int GetProgressHealthBonus()
        {
            var battleSettings = Core.Get<Settings>()?.Battle;
            var settings = battleSettings != null ? battleSettings.PlayerProgression : null;
            var saveManager = Core.Get<SaveManager>();
            var mapManager = Core.Get<MapManager>();
            if (settings == null || saveManager == null || mapManager == null)
                return 0;

            var location = mapManager.GetLocationNumber(saveManager.GetData<ProgressData>().Level);
            return settings.GetHealthBonus(location);
        }

        private void SetupArmor()
        {
            _armorData = null;
            if (!_useArmor)
                return;

            var armors = Core.Get<DataBase>().Armors;
            if (TryGetEquipped(InventoryCase.Armor, out var armorId, out var armorLevel))
            {
                _armorData = armors.GetData(armorId);
            }
            if (_armorData == null)
            {
                _armorData = armors.GetData(_armorType);
                armorLevel = _data.ArmorLevel;
            }
            _armorData.SetLevel(armorLevel);
            ApplyArmorVisuals(_armorData.InventoryId);
        }

        // Renderers linked to the equipped armor are shown, the rest of the linked renderers are hidden
        private void ApplyArmorVisuals(string armorId)
        {
            var visible = new HashSet<SkinnedMeshRenderer>(_armorRenderers
                .Where(link => link.InventoryId == armorId)
                .SelectMany(link => link.Renderers)
                .Where(r => r != null));

            foreach (var link in _armorRenderers)
            {
                foreach (var meshRenderer in link.Renderers)
                {
                    if (meshRenderer != null)
                        meshRenderer.enabled = visible.Contains(meshRenderer);
                }
            }
        }

        private SecondaryWeaponData GetSecondaryWeaponData()
        {
            var secondaryWeapons = Core.Get<DataBase>().SecondaryWeapons;
            SecondaryWeaponData data = null;
            if (TryGetEquipped(InventoryCase.SubWeapon, out var id, out var level))
            {
                data = secondaryWeapons.GetData(id);
            }
            if (data == null)
            {
                data = secondaryWeapons.GetData(_secondaryWeaponType);
                level = _data.SecondaryWeaponLevel;
            }
            data.SetLevel(level);
            return data;
        }

        // Equipment from the inventory overrides the serialized fallback (_weaponType/_armorType)
        private bool TryGetEquipped(InventoryCase inventoryCase, out string id, out int level)
        {
            id = null;
            level = 0;
            var inventory = Core.Get<InventoryManager>();
            if (inventory == null || !inventory.TryGetEquipped(inventoryCase, out var item, out level))
            {
                return false;
            }

            id = item.Id;
            return true;
        }

        public void RestoreEnergyShield()
        {
            _energyShield.Add(_energyShield.Max);
        }

        private void OnHitHandler()
        {
            OnHit?.Invoke();
            
        }
        
        private void OnFire()
        {
            OnShot?.Invoke();
        }

        private void OnAimLock(AimLockEvent lockEvent)
        {
            _isAimLock = lockEvent.IsLock;
        }

        public bool IsFire()
        {
            var state = _gameInput.Game.Fire.IsPressed() && !_isAimLock;
            if (_previousFireState != state)
            {
                if (!state)
                {
                    _secondaryWeaponController.OnFire();
                }
            }

            _previousFireState = state;
            return state;
        }

        public void BattleReady()
        {
            //_rig.enabled = false;
            _weaponRoot.gameObject.SetActive(true);
            _aim.SetActive(true);
            _weapon.SetActive(true, false);

            _gameInput.Enable();
            _battleReady = true;
            _cameraController.SetCamera(GameCameraType.Battle);
            _rig.Build();
            OnBattleStop += () =>
            {
                //_rig.enabled = false;
                _aim.SetActive(false);
                _weapon.SetActive(false, false);
                _rig.Build();
            };
            OnBattleReady?.Invoke(true);
        }
        public void BattleReady(BigWeapon bigWeapon)
        {
            //_rig.enabled = false;
            _weaponRoot.gameObject.SetActive(false);
            bigWeapon.Setup(this,IsFire, OnHitHandler);
            bigWeapon.SetActive(true);
            bigWeapon.Aim.Setup(_gameInput);
            _gameInput.Enable();
            _aim.SetAim(bigWeapon.ArmIkL, bigWeapon.ArmIkR, bigWeapon.Target);
            _battleReady = true;
            //_cameraController.SetCamera(GameCameraType.Battle);
            _cameraController.SetCamera(GameCameraType.BigWeapon, bigWeapon.CameraFollowTransform, bigWeapon.CameraTargetTransform);
            //_rig.enabled = true;
            _rig.Build();
            OnBattleStop += () =>
            {
                //_rig.enabled = false;
                bigWeapon.SetActive(false);
                _weaponRoot.gameObject.SetActive(true);
                _aim.SetActive(false);
                //_rig.enabled = true;
                _rig.Build();
            };
            OnBattleReady?.Invoke(true);
        }

        public void BattleStop()
        {
            _gameInput.Disable();
            _battleReady = false;
            OnBattleStop?.Invoke();
            OnBattleStop = null;
            OnBattleReady?.Invoke(false);
        }

        private void OnReactToDestinationPosition()
        {
            _viewer.Stop();
            OnDestinationTargetPosition?.Invoke();
            OnDestinationTargetPosition = null;
        }

        public void SetPosition(PlayerPosition positionSource)
        {
            _cameraController.SetCamera(GameCameraType.Run);
            _mover.TeleportTo(positionSource.Position, positionSource.Rotation);
        }
        
        public void MoveToPosition(PlayerPosition previewPosition, PlayerPosition targetPosition)
        {
            _cameraController.SetCamera(GameCameraType.Run);
            _mover.OnMoveCompleted += OnReactToDestinationPosition;
            _mover.OnFly += _viewer.Fly;
            _mover.MoveTo(targetPosition.Position, targetPosition.Rotation, previewPosition.Type!= PlayerPositionType.Start);
            PlayAnimationByPositionType(previewPosition.Type);
        }

        public void Victory()
        {
            PlayAnimationByPositionType(PlayerPositionType.End);
            EventBus.Push(new SoundPlayEvent(SoundType.Notification, transform.position), EventBus.EventRegion.GLOBAL);
        }
        
        private void PlayAnimationByPositionType(PlayerPositionType type)
        {
            Action viewerAction = type switch
            {
                PlayerPositionType.Start => _viewer.Run,
                PlayerPositionType.Default => _viewer.Jump,
                PlayerPositionType.End => _viewer.Victory,
                _ => _viewer.Run
            };
            viewerAction.Invoke();
        }
        
        public override void OnDamage(Team damageDealersTeam, int damageAmount, Vector3 hitPoint, List<IEffectAttribute> additiveAttributes = null)
        {
            // Own damage (e.g. explosion of the player's rocket) is ignored
            if (damageDealersTeam == Team || IsDead)
                return;

            OnDamaged?.Invoke();
            damageAmount = Mathf.Abs(damageAmount);

            // Energy shield takes damage first and ignores defense
            var absorbed = Mathf.Min(_energyShield.Current, damageAmount);
            _energyShield.Remove(absorbed);

            var healthDamage = damageAmount - absorbed;
            if (healthDamage > 0)
            {
                _health.Remove(Mathf.RoundToInt(healthDamage * (1f - DefenseFactor)));
            }
        }

        private void OnEnergyShieldChange(int delta)
        {
            if (delta < 0)
            {
                _damageReactionViewer?.Show(delta, transform.position, DamageTextType.Armor);
            }
        }
        
        public void OnHealthChange(int delta)
        {
            if (delta < 0)
            {
                _damageReactionViewer?.Show(delta, transform.position);
            }

            if (_health.Current <= 0)
            {
                Dead();
            }
        }

        private void Dead()
        {
            if (IsDead)
                return;

            IsDead = true;
            BattleStop();
            _rig.enabled = false;
            _viewer.Dead();
            _cameraController.SetCamera(GameCameraType.Dead);
            EventBus.Push(new PlayerDeadEvent(), EventBus.EventRegion.GAMEPLAY);
        }

        public void Revive()
        {
            if (!IsDead)
                return;

            IsDead = false;
            _health.Add(_health.Max);
            RestoreEnergyShield();
            _viewer.Revive();
            _cameraController.SetCamera(GameCameraType.Battle);
            EnableRigAfterRevive();
        }

        private async void EnableRigAfterRevive()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.PLAYER_REVIVE_IK_DELAY), cancellationToken: this.GetCancellationTokenOnDestroy());
                if (IsDead)
                    return;

                _rig.enabled = true;
                _rig.Build();
            }
            catch (OperationCanceledException)
            {
                // component destroyed mid-await, expected
            }
        }

        private void OnDisable()
        {
            OnBattleReady = null;
            OnHit = null;
            OnShot = null;
            OnSeccondaryShot = null;
            OnDestinationTargetPosition = null;
            OnDamaged = null;
            if (_weapon != null)
            {
                Weapon.Release(_weapon);
            }
            EventBus.Unsubscribe<AimLockEvent>(OnAimLock, EventBus.EventRegion.GAMEPLAY);
        }
        
#if UNITY_EDITOR
        [ContextMenu("ApplyAllDamageRepeaters")]
        private void ApplyAllDamageRepeaters()
        {
            _damageRepeaters = GetComponentsInChildren<DamageRepeater>();
            foreach (var damageRepeater in _damageRepeaters)
            {
                damageRepeater.ApplyReceiver(this);
                damageRepeater.FindColliders();
            }
        }
#endif
    }
}