namespace _GAME.Scripts.Events
{
    public class CameraShakeEvent: IEvent
    {
        public float shakeIntensity = 0;
        public float glitchIntensity = 0;
        public float duration = 0.2f;

        public CameraShakeEvent(float shake, float glitch, float dur) =>
            (shakeIntensity, glitchIntensity, duration) = (shake, glitch, dur) ;
    }
}