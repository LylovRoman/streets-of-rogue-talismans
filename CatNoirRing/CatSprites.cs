using System;
using System.IO;

namespace CatNoirRing
{
    internal static class CatSprites
    {
        public static byte[] Ring { get; } = Draw(PaintRing);
        public static byte[] Staff { get; } = Draw(PaintStaff);
        public static byte[] Effect { get; } = Draw(PaintEffect);
        public static byte[] Earrings { get; } = Draw(PaintEarrings);
        public static byte[] Yoyo { get; } = Draw(PaintYoyo);
        public static byte[] Charm { get; } = Draw(PaintCharm);
        public static byte[] MouseTalisman { get; } = Draw(PaintMouseTalisman);
        public static byte[] Rope { get; } = Draw(PaintRope);
        public static byte[] Multiply { get; } = Draw(PaintMultiply);
        public static byte[] BeeTalisman { get; } = Draw(PaintBeeTalisman);
        public static byte[] Trompo { get; } = Draw(PaintTrompo);
        public static byte[] Venom { get; } = Draw(PaintVenom);
        public static byte[] SnakeTalisman { get; } = Draw(PaintSnakeTalisman);
        public static byte[] SecondChance { get; } = Draw(PaintSecondChance);
        public static byte[] HorseTalisman { get; } = Draw(PaintHorseTalisman);
        public static byte[] Voyage { get; } = Draw(PaintVoyage);
        public static byte[] TigerTalisman { get; } = Draw(PaintTigerTalisman);
        public static byte[] Clout { get; } = Draw(PaintClout);
        public static byte[] DogTalisman { get; } = Draw(PaintDogTalisman);
        public static byte[] Ball { get; } = Draw(PaintBall);
        public static byte[] Fetch { get; } = Draw(PaintFetch);
        public static byte[] Mark { get; } = Draw(PaintMark);
        public static byte[] GoatTalisman { get; } = Draw(PaintGoatTalisman);
        public static byte[] Genesis { get; } = Draw(PaintGenesis);
        public static byte[] DragonTalisman { get; } = Draw(PaintDragonTalisman);
        public static byte[] DragonPower { get; } = Draw(PaintDragonPower);
        public static byte[] RoosterTalisman { get; } = Draw(PaintRoosterTalisman);
        public static byte[] Sublimation { get; } = Draw(PaintSublimation);
        public static byte[] OxTalisman { get; } = Draw(PaintOxTalisman);
        public static byte[] Resistance { get; } = Draw(PaintResistance);
        public static byte[] MonkeyTalisman { get; } = Draw(PaintMonkeyTalisman);
        public static byte[] Banana { get; } = Draw(PaintBanana);
        public static byte[] Confusion { get; } = Draw(PaintConfusion);
        public static byte[] MonkeyMark { get; } = Draw(PaintMonkeyMark);
        public static byte[] PeacockTalisman { get; } = Draw(PaintPeacockTalisman);
        public static byte[] Amok { get; } = Draw(PaintAmok);
        public static byte[] ButterflyTalisman { get; } = Draw(PaintButterflyTalisman);
        public static byte[] Akuma { get; } = Draw(PaintAkuma);
        public static byte[] TurtleTalisman { get; } = Draw(PaintTurtleTalisman);
        public static byte[] Shell { get; } = Draw(PaintShell);
        public static byte[] PigTalisman { get; } = Draw(PaintPigTalisman);
        public static byte[] Gift { get; } = Draw(PaintGift);
        public static byte[] FoxTalisman { get; } = Draw(PaintFoxTalisman);
        public static byte[] Mirage { get; } = Draw(PaintMirage);
        public static byte[] RabbitTalisman { get; } = Draw(PaintRabbitTalisman);
        public static byte[] TimeStop { get; } = Draw(PaintTimeStop);

        const int Size = 32;

        static byte[] Draw(Action<byte[], int> paint)
        {
            byte[] pixels = new byte[Size * Size * 4];
            paint(pixels, Size);
            return Encode(pixels, Size, Size);
        }

        static void PaintRing(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist <= 4.2f)
                        Put(pixels, size, x, y, 24, 220, 90, 255);
                    else if (dist <= 7.2f)
                        Put(pixels, size, x, y, 8, 8, 12, 255);
                    else if (dist <= 10.5f)
                        Put(pixels, size, x, y, 212, 170, 48, 255);
                    else if (dist <= 12.2f)
                        Put(pixels, size, x, y, 90, 68, 16, 255);
                }
            }
        }

        static void PaintStaff(byte[] pixels, int size)
        {
            for (int y = 2; y < size - 2; y++)
            {
                for (int x = 14; x <= 17; x++)
                    Put(pixels, size, x, y, 176, 184, 196, 255);
            }
            for (int y = 20; y < size - 3; y++)
            {
                for (int x = 13; x <= 18; x++)
                    Put(pixels, size, x, y, 18, 18, 22, 255);
            }
            for (int x = 12; x <= 19; x++)
            {
                Put(pixels, size, x, 3, 210, 216, 226, 255);
                Put(pixels, size, x, size - 3, 120, 126, 138, 255);
            }
        }

        static void PaintEffect(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center + 1f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool ear = (x >= 6 && x <= 11 && y >= 3 && y <= 10 && y <= 12 - x)
                        || (x >= 20 && x <= 25 && y >= 3 && y <= 10 && y <= x - 12);
                    if (ear || dist <= 10f)
                        Put(pixels, size, x, y, 10, 10, 14, 255);
                }
            }
            Fill(pixels, size, 10, 14, 13, 17, 36, 230, 96, 255);
            Fill(pixels, size, 18, 14, 21, 17, 36, 230, 96, 255);
        }

        static void PaintEarrings(byte[] pixels, int size)
        {
            Disc(pixels, size, 10, 16, 6.2f, 214, 24, 40, 255);
            Disc(pixels, size, 22, 16, 6.2f, 214, 24, 40, 255);
            Disc(pixels, size, 10, 16, 2.2f, 12, 12, 16, 255);
            Disc(pixels, size, 22, 16, 2.2f, 12, 12, 16, 255);
            Fill(pixels, size, 9, 6, 11, 10, 186, 150, 42, 255);
            Fill(pixels, size, 21, 6, 23, 10, 186, 150, 42, 255);
        }

        static void PaintYoyo(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 214, 24, 40, 255);
            Disc(pixels, size, center, center, 7f, 20, 20, 24, 255);
            Disc(pixels, size, center, center, 3.2f, 230, 230, 236, 255);
            for (int y = 2; y <= 8; y++)
                Put(pixels, size, 16, y, 230, 230, 236, 255);
        }

        static void PaintCharm(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 1f, 12f, 214, 24, 40, 255);
            Disc(pixels, size, center, center + 1f, 3f, 12, 12, 16, 255);
            Disc(pixels, size, center - 6f, center - 4f, 2.1f, 12, 12, 16, 255);
            Disc(pixels, size, center + 6f, center - 3f, 2.1f, 12, 12, 16, 255);
            Disc(pixels, size, center - 5f, center + 6f, 2.1f, 12, 12, 16, 255);
            Disc(pixels, size, center + 5f, center + 7f, 2.1f, 12, 12, 16, 255);
        }

        static void PaintMouseTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 176, 176, 186, 255);
            Disc(pixels, size, center, center + 2f, 4f, 240, 120, 168, 255);
            Disc(pixels, size, center - 4f, center - 2f, 1.4f, 20, 20, 24, 255);
            Disc(pixels, size, center + 4f, center - 2f, 1.4f, 20, 20, 24, 255);
            Fill(pixels, size, 8, 4, 12, 8, 240, 150, 186, 255);
            Fill(pixels, size, 19, 4, 23, 8, 240, 150, 186, 255);
        }

        static void PaintRope(byte[] pixels, int size)
        {
            for (int i = 4; i < size - 4; i++)
            {
                int x = 6 + (i % 5);
                Put(pixels, size, x, i, 240, 120, 168, 255);
                Put(pixels, size, x + 8, i, 240, 120, 168, 255);
            }
            Fill(pixels, size, 6, 4, 18, 7, 92, 92, 102, 255);
            Fill(pixels, size, 8, size - 8, 20, size - 5, 92, 92, 102, 255);
        }

        static void PaintMultiply(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 4f, 176, 176, 186, 255);
            Disc(pixels, size, center - 8f, center + 6f, 3.2f, 240, 150, 186, 255);
            Disc(pixels, size, center + 8f, center + 6f, 3.2f, 240, 150, 186, 255);
            Disc(pixels, size, center, center - 8f, 3.2f, 240, 150, 186, 255);
        }

        static void PaintBeeTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 240, 196, 32, 255);
            Fill(pixels, size, 8, 10, 23, 12, 16, 16, 18, 255);
            Fill(pixels, size, 8, 16, 23, 18, 16, 16, 18, 255);
            Fill(pixels, size, 8, 22, 23, 24, 16, 16, 18, 255);
        }

        static void PaintTrompo(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 2f, 8f, 240, 196, 32, 255);
            Disc(pixels, size, center, center + 2f, 3f, 16, 16, 18, 255);
            for (int y = 4; y <= 10; y++)
                Put(pixels, size, 16, y, 16, 16, 18, 255);
        }

        static void PaintVenom(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 2f, 8f, 150, 40, 210, 255);
            Disc(pixels, size, center, center + 4f, 3f, 40, 8, 64, 255);
        }

        static void PaintSnakeTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 32, 176, 168, 255);
            Disc(pixels, size, center, center, 7f, 8, 48, 52, 255);
            Disc(pixels, size, center, center - 1f, 2.2f, 240, 220, 60, 255);
            for (int i = -8; i <= 8; i++)
            {
                int x = (int)center + i;
                int y = (int)center + (i % 2 == 0 ? 6 : 4);
                Put(pixels, size, x, y, 240, 220, 60, 255);
            }
        }

        static void PaintSecondChance(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 12, 90, 96, 255);
            Disc(pixels, size, center, center, 6f, 32, 176, 168, 255);
            Disc(pixels, size, center, center, 2f, 240, 220, 60, 255);
        }

        static void PaintHorseTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 48, 28, 16, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - (center + 1f);
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool ring = dist <= 9.2f && dist >= 5.4f && y >= 8;
                    bool nail = (x >= 10 && x <= 12 && y >= 8 && y <= 12)
                        || (x >= 19 && x <= 21 && y >= 8 && y <= 12);
                    if (ring || nail)
                        Put(pixels, size, x, y, 212, 168, 48, 255);
                }
            }
        }

        static void PaintVoyage(byte[] pixels, int size)
        {
            Fill(pixels, size, 6, 6, 25, 26, 28, 18, 14, 255);
            Fill(pixels, size, 8, 8, 11, 24, 186, 150, 48, 255);
            Fill(pixels, size, 20, 8, 23, 24, 186, 150, 48, 255);
            Fill(pixels, size, 12, 10, 19, 22, 64, 196, 220, 255);
        }

        static void PaintTigerTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 232, 132, 28, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    if (dx * dx + dy * dy > 144f)
                        continue;
                    bool stripe = (y >= 8 && y <= 10) || (y >= 15 && y <= 17) || (y >= 22 && y <= 23);
                    if (stripe)
                        Put(pixels, size, x, y, 28, 16, 18, 255);
                }
            }

            Disc(pixels, size, center, center, 3.1f, 148, 42, 196, 255);
        }

        static void PaintDogTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 232, 122, 36, 255);
            Disc(pixels, size, center, center + 2f, 3.2f, 48, 24, 16, 255);
            Disc(pixels, size, center - 5f, center - 4f, 2.2f, 48, 24, 16, 255);
            Disc(pixels, size, center + 5f, center - 4f, 2.2f, 48, 24, 16, 255);
            Disc(pixels, size, center - 3.2f, center + 6f, 2.2f, 48, 24, 16, 255);
            Disc(pixels, size, center + 3.2f, center + 6f, 2.2f, 48, 24, 16, 255);
        }

        static void PaintBall(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 232, 140, 42, 255);
            Disc(pixels, size, center, center, 6.5f, 255, 214, 120, 255);
            for (int y = 6; y <= 25; y++)
            {
                int x = 16 + ((y / 2) % 2 == 0 ? 3 : -3);
                Put(pixels, size, x, y, 255, 248, 230, 255);
                Put(pixels, size, x + 1, y, 255, 248, 230, 255);
            }
        }

        static void PaintFetch(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center - 3f, 8f, 232, 140, 42, 255);
            Disc(pixels, size, center, center - 3f, 3f, 48, 24, 16, 255);
            Fill(pixels, size, 7, 20, 24, 26, 48, 24, 16, 255);
            Fill(pixels, size, 9, 16, 22, 20, 92, 52, 28, 255);
        }

        static void PaintMark(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 11f, 48, 24, 16, 255);
            Disc(pixels, size, center, center, 6.5f, 232, 140, 42, 255);
        }

        static void PaintGoatTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 1f, 11f, 186, 186, 196, 255);
            Fill(pixels, size, 8, 3, 12, 10, 236, 228, 204, 255);
            Fill(pixels, size, 19, 3, 23, 10, 236, 228, 204, 255);
            Disc(pixels, size, center - 4f, center - 1f, 1.3f, 42, 36, 32, 255);
            Disc(pixels, size, center + 4f, center - 1f, 1.3f, 42, 36, 32, 255);
            Disc(pixels, size, center, center + 4f, 1.6f, 232, 176, 64, 255);
        }

        static void PaintGenesis(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 186, 186, 196, 255);
            Fill(pixels, size, 11, 11, 20, 20, 255, 236, 160, 255);
            Fill(pixels, size, 14, 8, 17, 23, 255, 248, 220, 255);
            Fill(pixels, size, 8, 14, 23, 17, 255, 248, 220, 255);
        }

        static void PaintDragonTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12.2f, 92, 58, 16, 255);
            Disc(pixels, size, center, center, 9.4f, 176, 28, 36, 255);
            Disc(pixels, size, center, center - 1f, 4.2f, 255, 214, 96, 255);
            Fill(pixels, size, 15, 3, 16, 8, 255, 214, 96, 255);
        }

        static void PaintDragonPower(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 120, 18, 28, 255);
            Disc(pixels, size, center, center - 5f, 2.5f, 236, 236, 240, 255);
            Disc(pixels, size, center - 4.6f, center + 3.6f, 2.5f, 64, 156, 232, 255);
            Disc(pixels, size, center + 4.6f, center + 3.6f, 2.5f, 255, 214, 64, 255);
        }

        static void PaintRoosterTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 2f, 11f, 232, 112, 24, 255);
            Fill(pixels, size, 12, 2, 15, 9, 196, 32, 28, 255);
            Fill(pixels, size, 16, 1, 19, 8, 220, 48, 32, 255);
            Disc(pixels, size, center - 4f, center, 1.4f, 48, 24, 16, 255);
            Disc(pixels, size, center + 4f, center, 1.4f, 48, 24, 16, 255);
            Fill(pixels, size, 18, 16, 24, 19, 255, 196, 48, 255);
        }

        static void PaintSublimation(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 232, 112, 24, 255);
            Fill(pixels, size, 14, 6, 17, 25, 255, 236, 160, 255);
            Fill(pixels, size, 6, 14, 25, 17, 255, 236, 160, 255);
            Disc(pixels, size, center, center, 2.2f, 255, 248, 220, 255);
        }

        static void PaintOxTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 2f, 11f, 168, 96, 42, 255);
            Fill(pixels, size, 7, 3, 11, 12, 236, 214, 176, 255);
            Fill(pixels, size, 20, 3, 24, 12, 236, 214, 176, 255);
            Disc(pixels, size, center - 4f, center, 1.4f, 42, 24, 16, 255);
            Disc(pixels, size, center + 4f, center, 1.4f, 42, 24, 16, 255);
            Disc(pixels, size, center, center + 7f, 2.4f, 232, 184, 48, 255);
        }

        static void PaintResistance(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 168, 96, 42, 255);
            Fill(pixels, size, 14, 8, 17, 23, 255, 236, 210, 255);
            Fill(pixels, size, 8, 14, 23, 17, 255, 236, 210, 255);
            Disc(pixels, size, center, center, 2.2f, 232, 184, 48, 255);
        }

        static void PaintMonkeyTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center - 9f, center - 6f, 4.2f, 92, 52, 24, 255);
            Disc(pixels, size, center + 9f, center - 6f, 4.2f, 92, 52, 24, 255);
            Disc(pixels, size, center, center + 1f, 11f, 168, 104, 52, 255);
            Disc(pixels, size, center, center + 5f, 4.4f, 232, 196, 140, 255);
            Disc(pixels, size, center - 3.4f, center - 1f, 1.3f, 36, 18, 12, 255);
            Disc(pixels, size, center + 3.4f, center - 1f, 1.3f, 36, 18, 12, 255);
            Disc(pixels, size, center, center + 2.2f, 1.5f, 255, 214, 64, 255);
        }

        static void PaintBanana(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center - 1.5f, center + 1.5f, 12f, 255, 208, 32, 255);
            Disc(pixels, size, center + 6f, center - 5f, 10f, 0, 0, 0, 0);
            Fill(pixels, size, 14, 3, 18, 8, 110, 68, 28, 255);
            Disc(pixels, size, 15.5f, 22f, 1.6f, 92, 56, 16, 255);
        }

        static void PaintConfusion(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 92, 52, 24, 255);
            for (int i = 0; i < 18; i++)
            {
                float turn = i / 17f;
                float angle = turn * 5.4f;
                float radius = 3f + turn * 7.2f;
                float x = center + (float)Math.Cos(angle) * radius;
                float y = center + (float)Math.Sin(angle) * radius;
                Disc(pixels, size, x, y, 1.7f, 255, 214, 64, 255);
            }
        }

        static void PaintMonkeyMark(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 48, 28, 16, 255);
            Disc(pixels, size, center - 1f, center + 1f, 8f, 255, 208, 32, 255);
            Disc(pixels, size, center + 4f, center - 3f, 6.5f, 48, 28, 16, 255);
            Fill(pixels, size, 14, 6, 17, 10, 110, 68, 28, 255);
        }

        static void PaintPeacockTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center + 2f, 12f, 16, 110, 130, 255);
            Disc(pixels, size, center, center + 2f, 7.2f, 28, 170, 122, 255);
            Disc(pixels, size, center, center + 2f, 3.2f, 232, 186, 48, 255);
            Disc(pixels, size, center, center + 2f, 1.3f, 20, 42, 64, 255);
            Fill(pixels, size, 15, 1, 16, 8, 24, 150, 110, 255);
        }

        static void PaintAmok(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 12, 96, 118, 255);
            Disc(pixels, size, center, center, 6f, 232, 196, 64, 255);
            Disc(pixels, size, center, center, 2.2f, 20, 40, 64, 255);
        }

        static void PaintButterflyTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center - 8f, center - 2f, 7.2f, 148, 64, 196, 255);
            Disc(pixels, size, center + 8f, center - 2f, 7.2f, 148, 64, 196, 255);
            Disc(pixels, size, center - 7f, center + 6f, 5.2f, 196, 120, 224, 255);
            Disc(pixels, size, center + 7f, center + 6f, 5.2f, 196, 120, 224, 255);
            Fill(pixels, size, 15, 6, 16, 26, 42, 16, 64, 255);
            Disc(pixels, size, center - 2.2f, 5f, 1.2f, 42, 16, 64, 255);
            Disc(pixels, size, center + 2.2f, 5f, 1.2f, 42, 16, 64, 255);
            Disc(pixels, size, center, center - 1f, 1.5f, 255, 214, 96, 255);
        }

        static void PaintAkuma(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 72, 24, 112, 255);
            Disc(pixels, size, center - 6f, center - 1f, 4.4f, 186, 96, 220, 255);
            Disc(pixels, size, center + 6f, center - 1f, 4.4f, 186, 96, 220, 255);
            Fill(pixels, size, 15, 8, 16, 23, 24, 8, 36, 255);
            Disc(pixels, size, center, center, 2f, 255, 214, 96, 255);
        }

        static void PaintTurtleTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 36, 150, 72, 255);
            Disc(pixels, size, center, center + 1f, 7.4f, 18, 92, 48, 255);
            Disc(pixels, size, center, center + 1f, 3.2f, 150, 220, 120, 255);
            Fill(pixels, size, 8, 14, 10, 20, 236, 214, 90, 255);
            Fill(pixels, size, 21, 14, 23, 20, 236, 214, 90, 255);
        }

        static void PaintShell(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 120, 220, 140, 255);
            Disc(pixels, size, center, center, 8f, 16, 72, 40, 255);
            Disc(pixels, size, center, center, 3.2f, 190, 255, 200, 255);
        }

        static void PaintPigTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Fill(pixels, size, 6, 4, 12, 12, 255, 170, 206, 255);
            Fill(pixels, size, 19, 4, 25, 12, 255, 170, 206, 255);
            Disc(pixels, size, center, center + 2f, 11f, 255, 146, 196, 255);
            Disc(pixels, size, center, center + 5f, 4.2f, 255, 196, 220, 255);
            Disc(pixels, size, center - 1.8f, center + 5f, 1.1f, 196, 72, 120, 255);
            Disc(pixels, size, center + 1.8f, center + 5f, 1.1f, 196, 72, 120, 255);
            Disc(pixels, size, center - 4f, center - 1f, 1.3f, 72, 24, 48, 255);
            Disc(pixels, size, center + 4f, center - 1f, 1.3f, 72, 24, 48, 255);
        }

        static void PaintGift(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Fill(pixels, size, 7, 12, 24, 26, 255, 146, 196, 255);
            Fill(pixels, size, 14, 8, 17, 26, 255, 228, 240, 255);
            Fill(pixels, size, 7, 14, 24, 16, 255, 228, 240, 255);
            Disc(pixels, size, center - 4f, center - 6f, 3.2f, 255, 186, 214, 255);
            Disc(pixels, size, center + 4f, center - 6f, 3.2f, 255, 186, 214, 255);
            Disc(pixels, size, center, center - 4f, 1.6f, 255, 246, 250, 255);
        }

        static void PaintFoxTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center - 8f, center - 8f, 4.4f, 214, 92, 28, 255);
            Disc(pixels, size, center + 8f, center - 8f, 4.4f, 214, 92, 28, 255);
            Disc(pixels, size, center - 8f, center - 9.2f, 1.6f, 36, 16, 12, 255);
            Disc(pixels, size, center + 8f, center - 9.2f, 1.6f, 36, 16, 12, 255);
            Disc(pixels, size, center, center + 2f, 11f, 214, 92, 28, 255);
            Disc(pixels, size, center, center + 6f, 5.2f, 255, 236, 220, 255);
            Disc(pixels, size, center - 3.2f, center, 1.3f, 36, 16, 12, 255);
            Disc(pixels, size, center + 3.2f, center, 1.3f, 36, 16, 12, 255);
            Disc(pixels, size, center, center + 4.4f, 1.2f, 36, 16, 12, 255);
            Disc(pixels, size, center, center + 10f, 2.2f, 64, 196, 176, 255);
        }

        static void PaintMirage(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 64, 196, 176, 255);
            Disc(pixels, size, center, center, 8.4f, 214, 120, 48, 255);
            Disc(pixels, size, center, center, 4.2f, 255, 236, 196, 255);
            Fill(pixels, size, 6, 10, 25, 11, 220, 255, 248, 255);
            Fill(pixels, size, 8, 15, 23, 16, 220, 255, 248, 255);
            Fill(pixels, size, 10, 20, 21, 21, 220, 255, 248, 255);
        }

        static void PaintRabbitTalisman(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Fill(pixels, size, 7, 2, 12, 12, 255, 150, 186, 255);
            Fill(pixels, size, 19, 2, 24, 12, 255, 150, 186, 255);
            Fill(pixels, size, 9, 4, 11, 10, 255, 214, 228, 255);
            Fill(pixels, size, 20, 4, 22, 10, 255, 214, 228, 255);
            Disc(pixels, size, center, center + 3f, 11f, 214, 168, 64, 255);
            Disc(pixels, size, center, center + 3f, 8.4f, 255, 250, 246, 255);
            Fill(pixels, size, 15, 8, 16, 18, 48, 40, 64, 255);
            Fill(pixels, size, 15, 16, 22, 17, 48, 40, 64, 255);
            Disc(pixels, size, center, center + 3f, 1.5f, 255, 120, 160, 255);
        }

        static void PaintTimeStop(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 12f, 255, 140, 180, 255);
            Disc(pixels, size, center, center, 9f, 255, 250, 252, 255);
            Fill(pixels, size, 15, 6, 16, 16, 36, 32, 48, 255);
            Fill(pixels, size, 15, 15, 23, 16, 36, 32, 48, 255);
            Disc(pixels, size, center, center, 1.6f, 255, 120, 160, 255);
        }

        static void PaintClout(byte[] pixels, int size)
        {
            float center = (size - 1) / 2f;
            Disc(pixels, size, center, center, 13f, 148, 42, 196, 255);
            Disc(pixels, size, center, center + 1f, 7.2f, 232, 132, 28, 255);
            Fill(pixels, size, 12, 12, 19, 20, 255, 214, 160, 255);
            Fill(pixels, size, 3, 15, 8, 16, 255, 214, 160, 255);
            Fill(pixels, size, 23, 15, 28, 16, 255, 214, 160, 255);
        }

        static void Disc(byte[] pixels, int size, float cx, float cy, float radius, byte r, byte g, byte b, byte a)
        {
            int x0 = MathfFloor(cx - radius);
            int x1 = MathfCeil(cx + radius);
            int y0 = MathfFloor(cy - radius);
            int y1 = MathfCeil(cy + radius);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                        Put(pixels, size, x, y, r, g, b, a);
                }
            }
        }

        static int MathfFloor(float value)
        {
            int whole = (int)value;
            return value < whole ? whole - 1 : whole;
        }

        static int MathfCeil(float value)
        {
            int whole = (int)value;
            return value > whole ? whole + 1 : whole;
        }

        static void Fill(byte[] pixels, int size, int x0, int y0, int x1, int y1, byte r, byte g, byte b, byte a)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Put(pixels, size, x, y, r, g, b, a);
        }

        static void Put(byte[] pixels, int size, int x, int y, byte r, byte g, byte b, byte a)
        {
            if ((uint)x >= (uint)size || (uint)y >= (uint)size)
                return;
            int i = (y * size + x) * 4;
            pixels[i] = r;
            pixels[i + 1] = g;
            pixels[i + 2] = b;
            pixels[i + 3] = a;
        }

        public static byte[] Encode(byte[] rgba, int width, int height)
        {
            byte[] raw = new byte[height * (1 + width * 4)];
            for (int y = 0; y < height; y++)
            {
                int row = y * (1 + width * 4);
                raw[row] = 0;
                Buffer.BlockCopy(rgba, y * width * 4, raw, row + 1, width * 4);
            }

            using MemoryStream stream = new MemoryStream();
            stream.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);
            WriteChunk(stream, "IHDR", Header(width, height));
            WriteChunk(stream, "IDAT", ZlibStore(raw));
            WriteChunk(stream, "IEND", Array.Empty<byte>());
            return stream.ToArray();
        }

        static byte[] Header(int width, int height)
        {
            byte[] header = new byte[13];
            WriteInt(header, 0, width);
            WriteInt(header, 4, height);
            header[8] = 8;
            header[9] = 6;
            return header;
        }

        static byte[] ZlibStore(byte[] data)
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte(0x78);
            stream.WriteByte(0x01);
            int offset = 0;
            while (offset < data.Length)
            {
                int length = Math.Min(65535, data.Length - offset);
                bool last = offset + length >= data.Length;
                stream.WriteByte((byte)(last ? 1 : 0));
                stream.WriteByte((byte)(length & 0xFF));
                stream.WriteByte((byte)((length >> 8) & 0xFF));
                int complement = length ^ 0xFFFF;
                stream.WriteByte((byte)(complement & 0xFF));
                stream.WriteByte((byte)((complement >> 8) & 0xFF));
                stream.Write(data, offset, length);
                offset += length;
            }
            uint adler = Adler32(data);
            WriteInt(stream, adler);
            return stream.ToArray();
        }

        static void WriteChunk(Stream stream, string type, byte[] data)
        {
            WriteInt(stream, (uint)data.Length);
            byte[] name = new byte[4];
            for (int i = 0; i < 4; i++)
                name[i] = (byte)type[i];
            stream.Write(name, 0, 4);
            stream.Write(data, 0, data.Length);
            byte[] crcSource = new byte[4 + data.Length];
            Buffer.BlockCopy(name, 0, crcSource, 0, 4);
            Buffer.BlockCopy(data, 0, crcSource, 4, data.Length);
            WriteInt(stream, Crc32(crcSource));
        }

        static uint Adler32(byte[] data)
        {
            uint a = 1;
            uint b = 0;
            foreach (byte value in data)
            {
                a = (a + value) % 65521;
                b = (b + a) % 65521;
            }
            return (b << 16) | a;
        }

        static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte value in data)
            {
                crc ^= value;
                for (int i = 0; i < 8; i++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
            return ~crc;
        }

        static void WriteInt(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        static void WriteInt(Stream stream, uint value)
        {
            stream.WriteByte((byte)((value >> 24) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }
    }
}
