namespace _GAME.Scripts.Events
{
    public class CameraShakeEvent: IEvent
    {
        public float shakeIntensity = 0;
        public float darkDuration = 0f;
        public float glitchIntensity = 0;
        public float duration = 0f;

        public CameraShakeEvent(float shake, float darkDur, float glitch, float dur) =>
            (shakeIntensity, darkDuration, glitchIntensity, duration) = (shake, darkDur, glitch, dur) ;
    }
}