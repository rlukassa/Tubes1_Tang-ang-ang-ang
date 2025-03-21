using System;
using System.Drawing;
using System.Linq; 
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Bot Doremifasolasiuuu -- Made by Lukas 
// ------------------------------------------------------------------
// Penjelasan : 
// Bot ini menerapakan Algoritma Greedy untuk menyerang musuh. Namun lebih mengutamakan pada menembak musuh dengan peluru yang lebih banyak serta posisi musuh yang sekiranya mudah terkena peluru agar energi musuh cepat habis dan energi bot bertambah
// Bot ini akan bergerak ke area yang lebih sepi (dominan) dan menyerang musuh secara spray dari jarak jauh, namun tidak diam agar tidak mudah terkena peluru
// Corner yang dipilih secara acak pada awal permainan. Bot ini akan bergerak ke arah corner (dominan) tersebut dan bergerak ke arah musuh jika terdeteksi
// Untuk menyerang musuh, bot ini akan menembakkan peluru sesuai dengan jarak musuh yang terdeteksi. Bisa dengan diam atau bergerak ke arah musuh 
// namun tidak ada musuh yang terdeteksi, bot ini juga akan memperluas area scan dalama rentang tertentu
// dan jika terkena peluru dan terkena dinding, bot ini akan bergerak ke arah yang berlawanan
// ------------------------------------------------------------------
public class Lukas : Bot
{    
    private bool movingForward;

    int enemies; 
    int corner = RandomCorner(); 

    static void Main(string[] args)
    {
        new Lukas().Start(); 
    }

    Lukas() : base(BotInfo.FromFile("Lukas.json")) { }
    public override void Run()
    {
        BodyColor = Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A); // Warna Dark Raven Black
        TurretColor = Color.FromArgb(0xFF, 0x10, 0x20, 0x30); // Warna biru abu-abu seperti Dark Raven
        RadarColor = Color.Blue; // Warna radar biru
        BulletColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x66);  // Warna peluru Midnight (RGB: 0, 0, 102)
        ScanColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x80);  // Warna scan Navy (RGB: 0, 0, 128)

        enemies = EnemyCount;

        int gunIncrement = 3;

        while (IsRunning)
        {
            int noEnemyCounter = 0;
            for (int i = 0; i < 60; i++)
            {
                TurnGunRight(gunIncrement); 
                if (Events.OfType<ScannedBotEvent>().Any())
                {
                    noEnemyCounter = 0; 
                }
                else
                {
                    noEnemyCounter++; 
                }

                if (noEnemyCounter >= 5 * 30)
                {
                    gunIncrement = Math.Min(gunIncrement + 30, 180); 
                    noEnemyCounter = 0; 
                    MoveToLessCrowdedArea(); 
                }
            }
            gunIncrement *= -1;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        
        var bearingFromGun = GunBearingTo(e.X, e.Y);
        TurnGunLeft(bearingFromGun);
        if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

        if (bearingFromGun == 0)
            Rescan(); 
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        
        var bearing = CalcBearing(evt.Bullet.Direction); 
        TurnLeft(90 - bearing); 
        Back(5000);
    }

    public static int RandomCorner()
    {
        Random rand = new Random();
        return rand.Next(4) * 90; 
    }

    private void MoveToLessCrowdedArea()
    {
        var closestBot = GetClosestBot();
        if (closestBot != null)
        {
            var angleToEnemy = CalcBearingTo(closestBot.X, closestBot.Y);
            TurnLeft(angleToEnemy + 180);
            SetForward(100); 
            Forward(100);
        }
        else
        {
            MoveToCorner();
        }
    }

    private void MoveToCorner()
    {
        var angleToCorner = CalcBearingToCorner(corner);
        
        TurnLeft(angleToCorner);
        SetForward(1000);
        Forward(1000);
    }

    private double CalcBearingTo(double x, double y)
    {
        double dx = x - X;
        double dy = y - Y;
        return Math.Atan2(dy, dx) * (180 / Math.PI) - Direction;
    }

    private double CalcBearingToCorner(int corner)
    {
        double cornerX = 0, cornerY = 0;
        switch (corner)
        {
            case 0:
                cornerX = 0;
                cornerY = 0;
                break;
            case 90:
                cornerX = ArenaWidth;
                cornerY = 0;
                break;
            case 180:
                cornerX = ArenaWidth;
                cornerY = ArenaHeight;
                break;
            case 270:
                cornerX = 0;
                cornerY = ArenaHeight;
                break;
        }
        return CalcBearingTo(cornerX, cornerY);
    }

    private ScannedBotEvent GetClosestBot()
    {
        ScannedBotEvent closestBot = null;
        double closestDistance = double.MaxValue;
        foreach (var bot in Events.OfType<ScannedBotEvent>())
        {
            double distance = Math.Sqrt(Math.Pow(bot.X - X, 2) + Math.Pow(bot.Y - Y, 2));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBot = bot;
            }
        }
        return closestBot;
    }

       public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection(); 
    }

    private void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            SetForward(40000);
            movingForward = true;
        }
    }
}
