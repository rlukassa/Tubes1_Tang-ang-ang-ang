using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi.Events;

public class Reiner : Bot
{
    private Dictionary<int, (double x, double y, double energy)> enemyBots = new();
    private bool movingForward;

    static void Main(string[] args)
    {
        new Reiner().Start();
    }

    Reiner() : base(BotInfo.FromFile("Reiner.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        GunColor = Color.White;
        RadarColor = Color.White;
        ScanColor = Color.Yellow;

        movingForward = true;

        while (IsRunning)
        {
            TurnGunRight(360); 
            if (movingForward)
                Forward(100);
            else
                Back(100);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyBots[e.ScannedBotId] = (e.X, e.Y, e.Energy);
        double scannedDistance = DistanceTo(e.X, e.Y);
        if (scannedDistance < 100)
        {
            double bearingFromGun = GunBearingTo(e.X, e.Y);
            TurnGunLeft(bearingFromGun);
        
            double firepower = scannedDistance < 100 ? 3 : 2;
            Fire(firepower);
        }

        if (e.Energy <= 0)
        {
            enemyBots.Remove(e.ScannedBotId);
        }
        else
        {
            enemyBots[e.ScannedBotId] = (e.X, e.Y, e.Energy);
        }

        AttackLowestEnergy();

        }

    private void AttackLowestEnergy()
    {
        if (enemyBots.Count == 0) return;

        var target = GetLowestEnergy();

        double bearingFromGun = GunBearingTo(target.x, target.y);
        TurnGunLeft(bearingFromGun);

        double enemyDistance = DistanceTo(target.x, target.y);

        if(enemyDistance < 75){
            Fire(3);
        }else if(enemyDistance < 150){
            Fire(2);
        }else{
            Fire(1);
        }
    }

    private (double x, double y, double energy) GetLowestEnergy()
    {
        double minEnergy = double.MaxValue;
        double minDistance = double.MaxValue;
        double targetX = 0;
        double targetY = 0;
        double targetEnergy = 0;

        foreach (var enemy in enemyBots.Values)
        {
            double enemyDistance = DistanceTo(enemy.x, enemy.y);
            if (enemy.energy < minEnergy || (enemy.energy == minEnergy && enemyDistance < minDistance))
            {
                targetX = enemy.x;
                targetY = enemy.y;
                targetEnergy = enemy.energy;
                minEnergy = enemy.energy;
                minDistance = enemyDistance;
            }
        }
        return (targetX, targetY, targetEnergy);
    }

    public override void OnHitByBullet(HitByBulletEvent e){
        ReverseDirection();

        TurnRight(90);
        Forward(100);
        TurnLeft(90);
    }

    public override void OnHitBot(HitBotEvent e){
        var direction = DirectionTo(e.X, e.Y);
        var gunBearing = NormalizeRelativeAngle(direction - GunDirection);
        
        TurnGunLeft(gunBearing);
        Fire(2);
        Forward(200);
    }

    public override void OnHitWall(HitWallEvent e){
        ReverseDirection();
    }

    private void ReverseDirection(){
        if (movingForward)
        {
            Back(400);
            movingForward = false;
        }
        else
        {
            Forward(400);
            movingForward = true;
        }
    }
}