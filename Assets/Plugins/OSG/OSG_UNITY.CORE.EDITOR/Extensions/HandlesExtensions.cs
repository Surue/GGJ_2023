// Old Skull Games
// Benoit Constantin
// Tuesday, May 28, 2019


using UnityEditor;
using UnityEngine;

namespace OSG
{
    public static class HandlesExtensions
    {
        public static Vector3 DrawTarget(string title, Vector3 position,Quaternion rotation,Vector3 snap,Vector3 titleOffset, float circleRadius, float dotRadius, Color color)
        {
            Handles.color = color;
            Vector3 newPosition = Handles.FreeMoveHandle(position, rotation, circleRadius, snap, Handles.CircleHandleCap);
            Handles.SphereHandleCap(0, position, Quaternion.identity, dotRadius, EventType.Repaint);
            Handles.Label(newPosition + titleOffset, title);

            return newPosition;
        }


    }
}