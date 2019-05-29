using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Steganography
{
    class EllipticCurvesEncryption
    {
        static int p = 223; // модуль эллиптической кривой, часть открытого ключа, обязано быть простым числом
        static int d; // закрытый ключ
        static int[] P, Q, D;
        static int[] arrAB;
        public static string EncryptString(string stringToEncrypt)
        {
            string encryptedStr = "";
            //int d = 10; // то, на сколько умножать координаты; 1 < d < q - 1
            arrAB = GetAB(p); // часть открытого ключа
            Random rand = new Random();
            int x1 = 0, y1 = 0;
            int q; // порядок циклической подгруппы группы точек эллиптич. кривой
            do
            {
                x1 = 0;
                y1 = 0;
                while (true)
                {
                    if (Math.Pow(y1, 2) % p == (Math.Pow(x1, 3) + arrAB[0] * x1 + arrAB[1]) % p) // уравнение кривой
                        break;
                    x1 = rand.Next(1, p - 1);
                    y1 = rand.Next(1, p - 1);
                }
                P = new int[] { x1, y1 }; // часть открытого ключа
                q = Get_q(p, arrAB, x1, y1);
            } while (!isSimple(q));
            d = rand.Next(1, q - 1); // получаем рандомный закрытый ключ
            Q = NumMultiplyByCoord(p, arrAB[0], P, d); // часть открытого ключа

            // Encryption
            //int letter_id = 12;
            //encryptedStr = $"Pk = {Pk[0]}; {Pk[1]}; Qk = {Qk[0]}; {Qk[1]}; c = {c}; string to encrypt = {stringToEncrypt}";
            foreach(char c in stringToEncrypt)
            {
                encryptedStr += EncryptLetter(c, rand, arrAB[0], P, Q);
            }            

            return encryptedStr;
        }

        static string EncryptLetter(char letterToEncrypt, Random rand, int A, int[] P, int[] Q)
        {
            int letterToEncryptASCII = letterToEncrypt;
            string encryptedLetter = "";
            int rand_k = rand.Next(1, p - 1);
            //rand_k = 5;
            int[] Pk = NumMultiplyByCoord(p, A, P, rand_k);
            //Pk = new int[] { 25, 2 };
            int[] Qk = NumMultiplyByCoord(p, A, Q, rand_k);
            //Qk = new int[] { 3, 24 };
            double c = (letterToEncryptASCII * Qk[0]) % p;
            encryptedLetter = $"{Pk[0]};{Pk[1]};{c};";

            return encryptedLetter;
        }

        public static string DecryptString(string encryptedStr)
        {
            string decryptedStr = "";
            //int[] numbers = encryptedStr.Split(';').Select(snum => int.Parse(snum)).ToArray();
            int[] numbers = encryptedStr.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x)).ToArray();
            for(int i = 0; i < numbers.Count(); i += 3)
            {
                decryptedStr += DecryptLetter(numbers[i], numbers[i+1], numbers[i+2], arrAB[0]);
            }

            return decryptedStr;
        }

        static char DecryptLetter(int Pk0, int Pk1, int c, int A)
        {
            D = NumMultiplyByCoord(p, A, new int[]{ Pk0, Pk1 }, d);
            int xdminusone = 1;
            while ((D[0] * xdminusone) % p != 1)
            {
                xdminusone++;
            }
            //xdminusone = 14;
            double letterASCII = (c * xdminusone) % p;
            return (char)letterASCII;
        }

        static int[] GetAB(int p)
        {
            int[] result = new int[2];
            Random rand = new Random();
            result[0] = rand.Next(1, p - 1); // = A
            result[1] = rand.Next(1, p - 1); // = B

            while ((4 * Math.Pow(result[0], 3) + 27 * Math.Pow(result[1], 2)) % p == 0) // проверка чтобы A и B были такими что (4A^3 + 27B^2)mod(p) != 0     
            {
                result[0] = rand.Next(1, p - 1);
                result[1] = rand.Next(1, p - 1);
            }

            return result;
        }

        static int Get_q(int p, int[] arrAB, int x1, int y1)
        {
            int x2 = 0, y2 = 0, delta = 0, counter = 2;
            do
                delta++;
            while
                ((2 * y1 * delta) % p != (3 * Math.Pow(x1, 2) + arrAB[0]) % p);

            int tempx = (int)Math.Pow(delta, 2) - 2 * x1;
            while (tempx < p)
                tempx += p;
            x2 = tempx % p;
            int tempy = delta * (x1 - x2) - y1;
            while (tempy < p)
                tempy += p;
            y2 = tempy % p;
            int[] arrPrevXY = { x2, y2 };

            while (arrPrevXY[0] - x1 != 0)
            {
                arrPrevXY = GetXY(p, arrPrevXY, x1, y1);
                counter++;
            }

            counter++;
            return counter;
        }

        static int[] GetXY(int p, int[] arrPrevXY, int x1, int y1) // counts x and y after x2 and y2 (e.g. x3, x4, x5, ...)
        {
            int[] arrXY = new int[2];
            int delta_temp_x = arrPrevXY[0] - x1;
            while (delta_temp_x < p)
                delta_temp_x += p;
            int delta_temp_y = arrPrevXY[1] - y1;
            while (delta_temp_y < p)
                delta_temp_y += p;
            int delta = 0;
            while ((delta * delta_temp_x) % p != delta_temp_y % p)
                delta++;

            int arrX_temp = (int)(Math.Pow(delta, 2) - x1 - arrPrevXY[0]);
            while (arrX_temp < p)
                arrX_temp += p;
            arrXY[0] = arrX_temp % p;
            int arrY_temp = delta * (x1 - arrXY[0]) - y1;
            while (arrY_temp < p)
                arrY_temp += p;
            arrXY[1] = arrY_temp % p;

            return arrXY;
        }

        static int[] NumMultiplyByCoord(int p, int A, int[] coordArr, int n) // p - то, что стоит после модуля; A - начальное A; coordArr - xy координаты; n - на какое число умножать координаты
        {
            int[] newXYarr = new int[2];
            int delta = 1;
            delta = GetDelta(coordArr, coordArr, A, p);
            newXYarr = GetNewXAndYUsingDelta(coordArr, coordArr, delta, p);

            if (n > 2)
            {
                for (int i = 2; i < n; i++)
                {
                    delta = GetDelta(newXYarr, coordArr, A, p);
                    newXYarr = GetNewXAndYUsingDelta(newXYarr, coordArr, delta, p);
                }
            }

            return newXYarr;
        }

        static int GetDelta(int[] arrCurr, int[] arrStart, int A, int p)
        {
            int result = 1;

            if (arrCurr[0] == arrStart[0] && arrCurr[1] == arrCurr[1])
            {
                result = ModFromDiv((3 * (int)Math.Pow(arrCurr[0], 2) + A), (2 * arrCurr[1]), p);
            }
            else
            {
                result = ModFromDiv((arrCurr[1] - arrStart[1]), (arrCurr[0] - arrStart[0]), p);
            }

            return result;
        }

        static int ModFromDiv(int x, int y, int z) // (x/y)mod(z)
        {
            while (x < 0 || y < 0) // проверка на отрицательные значения в x-се или y-ке
            {
                x += z;
                y += z;
            }

            int d = 1, e = 0, f, g = z, h, i, j = y;
            for (i = 0; j > 0; i++)
            {
                try
                { h = y / g; }
                catch
                { h = 0; }

                j = y - h * g;
                if (i != 0)
                {
                    f = d * h + e;
                    e = d;
                    d = f;
                }
                y = g;
                g = j;
            }

            if (i % 2 != 0)
                e = z - e;
            try
            { d = (x * e) % z; }
            catch
            { d = (x * e); }
            return d;
        }

        static int[] GetNewXAndYUsingDelta(int[] arrCurr, int[] arrStart, int delta, int p)
        {
            int[] result = new int[2];
            int newX = (int)Math.Pow(delta, 2) - arrCurr[0] - arrStart[0];
            while (newX < p)
                newX += p;
            result[0] = newX % p;
            int newY = delta * (arrCurr[0] - result[0]) - arrCurr[1];
            while (newY < p)
                newY += p;
            result[1] = newY % p;
            return result;
        }

        static bool isSimple(int number)
        {
            bool prost = true;
            for(int i = 2; i <= number / 2; i++)
            {
                if(number % i == 0)
                {
                    prost = false;
                    break;
                }
            }

            return prost;
        }
    }
}
