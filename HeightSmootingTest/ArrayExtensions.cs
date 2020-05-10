using System;

namespace HeightSmootingTest
{
    public static class ArrayExtensions {
        public static void Fill<T>(this T[] originalArray, T with) {
            for(int i = 0; i < originalArray.Length; i++){
                originalArray[i] = with;
            }
        }  
        public static void Fill<T>(this T[] originalArray, T with, int times) {
            for(int i = 0; i < times; i++){
                originalArray[i] = with;
            }
        }  
        public static String _tostring<T>(this T[] originalArray)
        {
            String s = "";
            for(int i = 0; i < originalArray.Length; i++){
                s += originalArray[i];
            }

            return s;
        }  
    }
}