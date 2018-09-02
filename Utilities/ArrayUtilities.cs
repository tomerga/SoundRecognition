using System;

namespace SoundRecognition
{
    public static class ArrayUtilities
    {
        public static T[] SubArray<T>(this T[] data, int index, int length, bool isReverse)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            if(isReverse)
            {
                Array.Reverse(result);
            }

            return result;
        }
    }
}
