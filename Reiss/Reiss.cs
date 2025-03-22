using System; 
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Drawing;

// ------------------------------------------------------------------
// Bot Reiss -- Made by Lukas
// ------------------------------------------------------------------
// Penjelasan :
// Bot ini menggunakan Algoritma Greedy untuk menyerang musuh. Namun lebih mengutamakan pada jumlah energi bot
// Bot ini tetap akan menyerang musuh namun dengan tingkat kerusakan yang berbeda beda sesuai dengan jarak musuh yang terdeteksi
// Keluaran peluru juga akan disesuaikan dengan jarak musuh yang terdeteksi
// Ketika musuh sedikit, maka akan menggunakan sistem 1 peluru, jika musuh banyak, maka akan menggunakan sistem lebih dari 1 peluru
// Jika terserang peluru, bot ini akan bergerak ke arah yang berlawanan
// ------------------------------------------------------------------

public class Reiss : Bot
{
    static void Main(string[] args)
    {
        new Reiss().Start();
    }

    Reiss() : base(BotInfo.FromFile("Reiss.json")) { }


    public override void Run()
    {
        BodyColor = Color.FromArgb(0xFF, 0x6C, 0x9A, 0x5C); // Oregano Green (RGB: 108, 154, 92)
        TurretColor = Color.FromArgb(0xFF, 0x00, 0x52, 0x8A); // Rain Blue (RGB: 0, 82, 138)
        RadarColor = Color.Cyan;  // Warna radar Cyan
        BulletColor = Color.FromArgb(0xFF, 0x00, 0x38, 0x64);  // Azul Marino (RGB: 0, 56, 100)
        ScanColor = Color.Magenta;  // Warna scan Magenta

        while (IsRunning)
        {
            Forward(100);
            TurnGunRight(360);
            Back(100);
            TurnGunRight(360);
        }
    }

   
    public override void OnScannedBot(ScannedBotEvent e)
    {

        var bearingFromGun = GunBearingTo(e.X, e.Y); 
        double distance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2)); 

        TurnGunLeft(bearingFromGun); 

        if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1)); 

        if (bearingFromGun == 0)
            Rescan(); 
    
        SmartFire(distance, e.Energy); 
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {

        var bearing = CalcBearing(evt.Bullet.Direction);
       
        TurnLeft(270 - bearing);
        Back(3000);
    }

    private void SmartFire(double distance, double enemyEnergy)
    {
   
        double firePower;
        if (distance > 200)
        {
            firePower = 1; 
        }
        else if (distance > 50)
        {
            firePower = 2; 
        }
        else
        {
            firePower = 3;
        }

       
        if (Energy < 15)
        {
            firePower = Math.Min(firePower, 1); 
        }

       
        if (enemyEnergy < 10)
        {
            firePower = Math.Min(firePower, 1); 
        }

       
        Fire(firePower);
    }
}