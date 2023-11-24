using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Backend
{
    public class RandomNumberGenerator
    {

        private int currentRunningIndex;
        private uint originalSeed;
        private uint currentSeed;

        static uint multiplier = 1103515245;
        static uint increment = 12345;

        static uint modulus = uint.MaxValue;

        static uint lcg(uint a, uint c, uint m, uint seed) => seed = (a * seed + c) % m;

        private Func<uint, uint, uint, uint, uint> gen;
        private double number;
        private double rmod;
        private double percForEachIds;
        private double randomValue;
        private double currentThreshold;

        public RandomNumberGenerator(uint seed) {
            originalSeed = seed;
            currentSeed = seed;
            currentRunningIndex = 0;
            gen = (Func<uint, uint, uint, uint, uint>)(lcg);
        }

        public RandomNumberGenerator(uint seed, int runningIndex) {
            originalSeed = seed;
            currentSeed = seed;
            currentRunningIndex = runningIndex;
            gen = (Func<uint, uint, uint, uint, uint>)(lcg);

            generateNumber(currentSeed, currentRunningIndex);
        }

        public double RandomValue(){
            return randomNumberOnce();
        }

        public int RandomIndex(int poolSize){
            return randomValueWithinRange(poolSize);
        }

        private double generateNumber(uint seed, int runningIndex)
        {
            number = 0f;
            for (int i = 0; i < runningIndex; i++)
            {
                seed = gen(multiplier, increment, modulus, seed);
                rmod = Convert.ToDouble(seed % (double)1000000000);
                number = rmod / 1000000000f;
            }
            return number;
        }

        private double randomNumberOnce(){
            number = 0f;

            currentSeed = gen(multiplier, increment, modulus, currentSeed);
            rmod = (double)currentSeed % (double)1000000000;
            number = rmod / 1000000000f;

            currentRunningIndex += 1;

            return number;
        }

        private int randomValueWithinRange(int randomRange){
            percForEachIds = (double) 1.0f / (double) randomRange;
            randomValue = randomNumberOnce();
            currentThreshold = percForEachIds;

            for(int i = 0; i < randomRange; i++){
                if(randomValue <= currentThreshold){
                    return i;
                }
                currentThreshold += percForEachIds;
            }
            return  randomRange - 1;
        }

        // private int randomValueWithinRange(uint randomRange) {
        //     currentSeed = gen(multiplier, increment, modulus, currentSeed);
        //     ulong m = Convert.ToUInt64(currentSeed) * Convert.ToUInt64(randomRange);
        //     Debug.Log($"m: {m}");
        //     uint l = Convert.ToUInt32(m);

        //     if(l < randomRange){
        //         uint t = Convert.ToUInt32(-randomRange);
        //         if(t >= randomRange){
        //             t -= randomRange;
        //             if(t >= randomRange){
        //                 t %= randomRange;
        //             }
        //         }

        //         while (l < t){
        //             currentSeed = gen(multiplier, increment, modulus, currentSeed);
        //             m = Convert.ToUInt64(currentSeed) * Convert.ToUInt64(randomRange);
        //             l = Convert.ToUInt32(m);
        //         }
        //     }

        //     return Convert.ToInt32((m >> 32));
        // }
    }
}
