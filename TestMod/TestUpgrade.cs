using DisfigureModApi;
using DisfigureModApi.Modules;
using DisfigureModApi.UpgradeCreationTools;
using UnityEngine;

namespace TestMod;

public class TestUpgrade : NewUpgrade
{
    public override void BuildUpgradeTree(PlayerStats pS)
    {
        TestPlugin.Log.LogMessage("Constructing Upgrades");

        List<GameObject> list = new List<GameObject>();
        GameObject gameObject = pS.BuildUpgrade("Last bottom Upgrade", "This is the last bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.3f
        }, null, null, new DesclinesWrapper { UpperLine = "man i love to <color=red> shoot </color> things" }, spriteUpgradeName: "U.Plague", spriteColor: Color.red);
        GameObject gameObject2 = pS.BuildUpgrade("Middle bottom Upgrade", "This is the middle bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.2f
        }, gameObject, null, new DesclinesWrapper
        {
            UpperLine = "i could use a <color=blue> coke </color> right now"
        }, spriteUpgradeName: "U.BlindingLight", spriteColor: Color.blue);
        GameObject gameObject3 = pS.BuildUpgrade("First bottom Upgrade", "This is the first bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.1f
        }, gameObject2, null, new DesclinesWrapper
        {
            UpperLine = "wait... youre actually <color=white> Looking </color> at this description?"
        }, spriteUpgradeName: "U.DeadlyRicochets", spriteColor: Color.green);
        GameObject gameObject4 = pS.BuildUpgrade("Last top Upgrade", "This is the last top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.3f
        }, null, null, new DesclinesWrapper
        {
            UpperLine = "So now you know i am on <color=white> Top </color>"
        }, spriteUpgradeName: "U.GuidedLaser", spriteColor: Color.yellow);
        GameObject gameObject5 = pS.BuildUpgrade("Middle top Upgrade", "This is the middle top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.2f
        }, gameObject4, null, new DesclinesWrapper
        {
            UpperLine = "I am the <color=red> danger </color> ",
            LowerLine = "i am in..... <color=white> C O N T R O L </color>"
        }, spriteUpgradeName: "U.RapidFireRadiation", spriteColor: Color.cyan);
        GameObject gameObject6 = pS.BuildUpgrade("First top Upgrade", "This is the first top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.1f
        }, gameObject5, null, new DesclinesWrapper
        {
            UpperLine = "I am the <color=white> ONE </color> who knocks!"
        }, spriteUpgradeName: "U.EightfoldRicochet", spriteColor: Color.magenta);
        GameObject gameObject7 = pS.BuildUpgrade("Initial Upgrade", "This is the initial upgrade", new UpgradeStatWrapper
        {
            name = "wandprojectileturnspeed",
            value = 0.25f
        }, gameObject6, gameObject3, new DesclinesWrapper
        {
            UpperLine = "This is the start of something <color=yellow> great! </color> ",
            LowerLine = "As long as you have enough <color=red> rubies </color>"
        }, spriteUpgradeName: "U.QuickRadiation", spriteColor: Color.white);
        list.Add(gameObject6);
        list.Add(gameObject5);
        list.Add(gameObject4);
        list.Add(gameObject3);
        list.Add(gameObject2);
        list.Add(gameObject);
        pS.AssignInitialUpgrade(gameObject7, list);
    }
}