// Flavien CASTON
// 04 10 2020

using UnityEngine;

public static class EaseExtensions
{
    public static AnimationCurve FadeInFadeOutCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
}