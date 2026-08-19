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
        GameObject gameObject = UpgradeUtils.BuildUpgrade(pS, "Last bottom Upgrade", "This is the last bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.3f
        }, null, null, new DesclinesWrapper
        {
            UpperLine = "man i love to <color=red> shoot </color> things"
        });
        GameObject gameObject2 = UpgradeUtils.BuildUpgrade(pS, "Middle bottom Upgrade", "This is the middle bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.2f
        }, gameObject, null, new DesclinesWrapper
        {
            UpperLine = "i could use a <color=blue> coke </color> right now"
        });
        GameObject gameObject3 = UpgradeUtils.BuildUpgrade(pS, "First bottom Upgrade", "This is the first bottom upgrade", new UpgradeStatWrapper
        {
            name = "projspeed",
            value = 0.1f
        }, gameObject2, null, new DesclinesWrapper
        {
            UpperLine = "wait... youre actually <color=white> Looking </color> at this description?"
        });
        GameObject gameObject4 = UpgradeUtils.BuildUpgrade(pS, "Last top Upgrade", "This is the last top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.3f
        }, null, null, new DesclinesWrapper
        {
            UpperLine = "So now you know i am on <color=white> Top </color>"
        });
        GameObject gameObject5 = UpgradeUtils.BuildUpgrade(pS, "Middle top Upgrade", "This is the middle top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.2f
        }, gameObject4, null, new DesclinesWrapper
        {
            UpperLine = "I am the <color=red> danger </color> ",
            LowerLine = "i am in..... <color=white> C O N T R O L </color>"
        });
        GameObject gameObject6 = UpgradeUtils.BuildUpgrade(pS, "First top Upgrade", "This is the first top upgrade", new UpgradeStatWrapper
        {
            name = "damage",
            value = 0.1f
        }, gameObject5, null, new DesclinesWrapper
        {
            UpperLine = "I am the <color=white> ONE </color> who knocks!"
        });
        GameObject gameObject7 = UpgradeUtils.BuildUpgrade(pS, "Initial Upgrade", "This is the initial upgrade", new UpgradeStatWrapper
        {
            name = "wandprojectileturnspeed",
            value = 0.25f
        }, gameObject6, gameObject3, new DesclinesWrapper
        {
            UpperLine = "This is the start of something <color=yellow> great! </color> ",
            LowerLine = "As long as you have anough <color=red> rubies </color>"
        });
        list.Add(gameObject6);
        list.Add(gameObject5);
        list.Add(gameObject4);
        list.Add(gameObject3);
        list.Add(gameObject2);
        list.Add(gameObject);
        UpgradeUtils.AssignInitialUpgrade(pS, gameObject7, list);
    }
}