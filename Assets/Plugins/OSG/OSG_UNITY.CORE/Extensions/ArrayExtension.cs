// Old Skull Games
// Benoit Constantin
// Monday, June 10, 2019


using System;

public static class ArrayExtension {

    public static Array RemoveAt(Array array, int index)
    {
        int end = array.Length - 1;
        for (int i = index; i < end; i++)
        {
            array.SetValue(array.GetValue(i + 1),i);
        }
        return ResizeArray(array, array.Length - 1);
    }

    public static System.Array ResizeArray(this System.Array oldArray, int newSize)
    {
        int oldSize = oldArray.Length;
        System.Type elementType = oldArray.GetType().GetElementType();
        System.Array newArray = System.Array.CreateInstance(elementType, newSize);
        int preserveLength = System.Math.Min(oldSize, newSize);
        if (preserveLength > 0)
        {
            System.Array.Copy(oldArray, newArray, preserveLength);
        }
        return newArray;
    }
}