using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class PhaseBot : Bot
{
    private bool movingForward;
    private double enemyDistance;
    private double currentEnemyX = 0;
    private double currentEnemyY = 0;

    private int previousTurnNumber;
    private double previousCurrentEnemyX;
    private double previousCurrentEnemyY;

    int corner = RandomCorner();

    static void Main(string[] args)
    {
        new PhaseBot().Start();
    }

    PhaseBot() : base(BotInfo.FromFile("PhaseBot.json")) { }

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
            Forward(100);
            Back(100);
            TurnGunRight(360);
        }
    }

    // Periksa energi dan tentukan fase
    private void CheckEnergyAndSwitchPhase()
    {
        if (Energy >= 80)
        {
            AttackPhase();
        }
        else if (Energy < 75 && Energy >= 40)
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

        if (enemyDistance > 0)
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
        int currentTurnNumber = TurnNumber;

        if (previousTurnNumber > 0)
        {
            double deltaTime = currentTurnNumber - previousTurnNumber;
            double velocityX = (currentEnemyX - previousCurrentEnemyX) / deltaTime;
            double velocityY = (currentEnemyY - previousCurrentEnemyY) / deltaTime;
            enemyDistance = DistanceTo(e.X, e.Y);

            double bulletSpeed = CalcBulletSpeed(3);

            double timeToTarget = enemyDistance / bulletSpeed;
            double futureX = currentEnemyX + velocityX * timeToTarget;
            double futureY = currentEnemyY + velocityY * timeToTarget;

            double gunBearing = GunBearingTo(futureX, futureY);
            GunTurnRate = gunBearing;

            Fire(3);

            if (enemyDistance <= 200)
            {
                Fire(3);
            }
            else if (enemyDistance <= 400)
            {
                Fire(2);
            }
            else
            {
                Fire(1);
            }
        }

        previousCurrentEnemyX = currentEnemyX;
        previousCurrentEnemyY = currentEnemyY;
        previousTurnNumber = currentTurnNumber;

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
};