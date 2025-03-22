using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Yeager : Bot
{
    private bool movingForward;
    private double currentEnemyX = 0;
    private double currentEnemyY = 0;

    private int corner;

    static void Main(string[] args)
    {
        new Yeager().Start();
    }

    Yeager() : base(BotInfo.FromFile("Yeager.json"))
    {
        corner = RandomCorner(); // Inisialisasi corner di constructor
    }

    public override void Run()
    {
        // Set warna bot
        BodyColor = Color.Blue;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.Green;

        movingForward = true;

        while (IsRunning)
        {
            CheckEnergyAndSwitchPhase(); // Periksa energi dan tentukan fase
            MoveToSafeZone(); // Bergerak ke zona aman
            TurnGunRight(360); // Selalu scanning
        }
    }

    // Periksa energi dan tentukan fase
    private void CheckEnergyAndSwitchPhase()
    {
        if (Energy >= 80)
        {
            AttackPhase();
        }
        else if (Energy < 75 && Energy >= 25)
        {
            DefensivePhase();
        }
        else
        {
            NothingToLosePhase();
        }
    }

    // Attack Phase: Fokus menyerang musuh
    private void AttackPhase()
    {
        Forward(100);
        TurnGunRight(360);
    }

    // Defensive Phase: Bergerak ke dinding terdekat
    private void DefensivePhase()
    {
        TurnGunRight(360);

        if (currentEnemyX != 0 && currentEnemyY != 0) // Pastikan ada data musuh
        {
            double bearingFromBot = BearingTo(currentEnemyX, currentEnemyY);
            double moveDirection = NormalizeRelativeAngle(bearingFromBot + 180);

            TurnLeft(moveDirection - Direction);
            Forward(100);
        }
    }

    // NothingToLose Phase: Bergerak acak dan tembak musuh
    private void NothingToLosePhase()
    {
        TurnGunRight(360);
        Fire(1);
    }

    // Saat mendeteksi musuh
    public override void OnScannedBot(ScannedBotEvent e)
    {
        currentEnemyX = e.X;
        currentEnemyY = e.Y;
        var bearingFromGun = GunBearingTo(e.X, e.Y);
        TurnGunLeft(bearingFromGun);

        if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

        if (bearingFromGun == 0)
            Rescan();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
        TurnRight(180);
        TurnLeft(90);
        TurnGunRight(360);
    }

    // Balik arah
    private void ReverseDirection()
    {
        if (movingForward)
        {
            Back(40000);
            movingForward = false;
        }
        else
        {
            Forward(40000);
            movingForward = true;
        }
    }

    // Bergerak ke zona aman
    private void MoveToSafeZone()
    {
        if (Energy < 40)
        {
            double safeX = ArenaWidth / 4 + (corner % 2) * ArenaWidth / 2;
            double safeY = ArenaHeight / 4 + (corner / 2) * ArenaHeight / 2;

            double bearingToSafeZone = BearingTo(safeX, safeY);
            double moveDirection = NormalizeRelativeAngle(bearingToSafeZone - Direction);

            TurnLeft(moveDirection);
            Forward(100);
        }
    }

    // Menghasilkan sudut acak untuk zona aman
    public int RandomCorner()
    {
        return new Random().Next(4);
    }
}