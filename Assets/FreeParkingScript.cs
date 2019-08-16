using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class FreeParkingScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable tokenButton;
    public KMSelectable goButton;
    public KMSelectable jailButton;
    public KMSelectable freeParkingButton;
    public GameObject startScreen;
    public GameObject mainScreen;
    public GameObject tokenObject;
    public MoneyInformation[] startMoney;

    public Material[] backgroundMaterials;
    public Renderer surfaceRend;
    public Renderer[] paidMoneyRend;
    public Renderer tokenRend;
    public Material[] tokenOptions;
    private int tokenIndex = 0;

    public Renderer finishCard;
    public Renderer finishCard2;
    public Material[] finishMatOptions;

    private int moneyStage = 0;
    private int paidMoney = 0;

    private String baseMoney = "";
    private String baseMoneyEdit = "";
    private int baseMoneyInt = 0;
    private int baseMoneyIntOriginal = 0;

    private int laundryCookingSinkCount = 0;
    private int hieroglyphicsLionCount = 0;
    private int maintenanceGridCount = 0;
    private int britishLondonCount = 0;
    private int dalmatiansCount = 0;
    private int retirementCount = 0;
    private int battleFlagSemaphoreMorseCount = 0;

    public int[] moduleValues;
    private int[] moduleIncreaser = new int [7];

    private List<string> moduleNames = new List<string>();
    private List<string> solvedModulesNames = new List<string>();

    private int solvedModules = 0;
    private int strikeCount = 0;
    private int tempSolvedModules = 0;
    private int tempStrikeCount = 0;
    public int[] moduleSolveValues;
    private int subtraction = 0;
    private bool burglarSillyCheapPresent;
    private bool note3Solved;
    private bool pressJail;
    private bool valueGrounded;
    private int moduloMoney = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (MoneyInformation money in startMoney)
        {
            MoneyInformation pressedMoney = money;
            money.selectable.OnInteract += delegate () { StartMoneyPress(pressedMoney); return false; };
        }
        tokenButton.OnInteract += delegate () { PressToken(); return false; };
        goButton.OnInteract += delegate () { PressGo(); return false; };
        jailButton.OnInteract += delegate () { PressJail(); return false; };
        freeParkingButton.OnInteract += delegate () { PressFreeParking(); return false; };
    }

    void Start()
    {
        foreach(Renderer money in paidMoneyRend)
        {
            money.enabled = false;
        }
        finishCard.enabled = false;
        finishCard2.enabled = false;
        mainScreen.SetActive(false);
        surfaceRend.material = backgroundMaterials[0];
        tokenIndex = UnityEngine.Random.Range(0,7);
        tokenRend.material = tokenOptions[tokenIndex];
        moduleNames = Bomb.GetModuleNames();
        Debug.LogFormat("[Free Parking #{0}] Your token is the {1}.", moduleId, tokenOptions[tokenIndex].name);
        if(moduleNames.Where((x) => x.Equals("The Jewel Vault")).Count() >= 1 && moduleNames.Where((x) => x.Equals("Silly Slots")).Count() >= 1 && moduleNames.Where((x) => x.Equals("Cheap Checkout")).Count() >= 1)
        {
            //Will now check for completion of Jewel/Silly Slots/Cheap Checkout and initiate bankruptcy when solved
            Debug.LogFormat("[Free Parking #{0}] The Jewel Vault, Silly Slots & Cheap Checkout are present. When these modules are solved, Free Parking will be bankrupt and the value will be $0.", moduleId);
            burglarSillyCheapPresent = true;
        }

        if ((moduleNames.Where((x) => x.Equals("Burglar Alarm")).Count() >= 1 && moduleNames.Where((x) => x.Equals("Safety Safe")).Count() >= 1) || (moduleNames.Where((x) => x.Equals("Burglar Alarm")).Count() >= 1 && moduleNames.Where((x) => x.Equals("The Jewel Vault")).Count() >= 1) || (moduleNames.Where((x) => x.Equals("The Jewel Vault")).Count() >= 1 && moduleNames.Where((x) => x.Equals("Safety Safe")).Count() >= 1) || (moduleNames.Where((x) => x.Contains("Double")).Count() > 2))
        {
            //Go to jail correct response due to Burglar/Safety/Jewel presence or three doubles
            pressJail = true;
            Debug.LogFormat("[Free Parking #{0}] You have 2/3 of Burglar Alarm, Safety Safe and The Jewel Vault or have three 'double' modules. Go to jail. Go directly to jail. Do not pass GO! Do not collect $200. Solve the module by pressing the 'Go To Jail' button.", moduleId);
            if(burglarSillyCheapPresent)
            {
                burglarSillyCheapPresent = false;
                Debug.LogFormat("[Free Parking #{0}] This has negated the presence of The Jewel Vault, Silly Slots & Cheap Checkout. Solving these modules will no longer affect Free Parking.", moduleId);
            }
        }
        else if(Bomb.IsIndicatorOn("BOB") || Bomb.IsIndicatorOff("BOB"))
        {
            //Money is always $0 due to BOB
            baseMoneyInt = 0;
            valueGrounded = true;
            Debug.LogFormat("[Free Parking #{0}] Bob is present. Bob likes the original rules. Your base money is $0.", moduleId);
        }
        else
        {
            GetBaseMoney();
            RelevantModuleCount();
            AddModuleValues();
        }
    }

    void GetBaseMoney()
    {
        baseMoney = Bomb.GetSerialNumber();
        foreach(char character in baseMoney)
        {
            if(character == '0' || character == '1' || character == '2' || character == '3' || character == '4' || character == '5' || character == '6' || character == '7' || character == '8' || character == '9')
            {
                baseMoneyEdit += character;
            }
        }
        baseMoneyInt = Int32.Parse(baseMoneyEdit);
        Debug.LogFormat("[Free Parking #{0}] Your base money is ${1}.", moduleId, baseMoneyInt);
    }

    void RelevantModuleCount()
    {
        for(int i = 0; i < moduleNames.Count; i++)
        {
            if(moduleNames[i] == "Laundry" || moduleNames[i] == "Cooking" || moduleNames[i] == "Sink")
            {
                laundryCookingSinkCount++;
            }
            if(moduleNames[i] == "Hieroglyphics" || moduleNames[i] == "Lion’s Share" || moduleNames[i] == "Lion's Share")
            {
                hieroglyphicsLionCount++;
            }
            if(moduleNames[i] == "Maintenance" || moduleNames[i] == "Gridlock")
            {
                maintenanceGridCount++;
            }
            if(moduleNames[i] == "British Slang" || moduleNames[i] == "The London Underground")
            {
                britishLondonCount++;
            }
            if(moduleNames[i] == "101 Dalmatians")
            {
                dalmatiansCount++;
            }
            if(moduleNames[i] == "Retirement")
            {
                retirementCount++;
            }
            if(moduleNames[i] == "Battleship" || moduleNames[i] == "Maritime Flags" || moduleNames[i] == "Semaphore" || moduleNames[i] == "Morse War")
            {
                battleFlagSemaphoreMorseCount++;
            }
        }
        /*/
        Module count for testing purposes
        laundryCookingSinkCount = 1;
        hieroglyphicsLionCount = 1;
        maintenanceGridCount = 0;
        britishLondonCount = 1;
        dalmatiansCount = 2;
        retirementCount = 0;
        battleFlagSemaphoreMorseCount = 3;/*/
    }

    void AddModuleValues()
    {
        moduleIncreaser[0] = (moduleValues[7 * tokenIndex] * laundryCookingSinkCount);
        moduleIncreaser[1] = (moduleValues[(7 * tokenIndex) + 1] * hieroglyphicsLionCount);
        moduleIncreaser[2] = (moduleValues[(7 * tokenIndex) + 2] * maintenanceGridCount);
        moduleIncreaser[3] = (moduleValues[(7 * tokenIndex) + 3] * britishLondonCount);
        moduleIncreaser[4] = (moduleValues[(7 * tokenIndex) + 4] * dalmatiansCount);
        moduleIncreaser[5] = (moduleValues[(7 * tokenIndex) + 5] * retirementCount);
        moduleIncreaser[6] = (moduleValues[(7 * tokenIndex) + 6] * battleFlagSemaphoreMorseCount);
        int totalIncrease = moduleIncreaser.Sum();
        baseMoneyInt = baseMoneyInt + totalIncrease;
        if(baseMoneyInt < 0)
        {
            baseMoneyInt = 0;
        }
        Debug.LogFormat("[Free Parking #{0}] There are {1} Laundry/Cooking/Sink modules, for a total increase of ${2}.", moduleId, laundryCookingSinkCount, moduleIncreaser[0]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} Hierogyphics/Lion's Share modules, for a total increase of ${2}.", moduleId, hieroglyphicsLionCount, moduleIncreaser[1]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} Maintenance/Gridlock modules, for a total increase of ${2}.", moduleId, maintenanceGridCount, moduleIncreaser[2]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} British Slang/The London Underground modules, for a total increase of ${2}.", moduleId, britishLondonCount, moduleIncreaser[3]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} 101 Dalmatians modules, for a total increase of ${2}.", moduleId, dalmatiansCount, moduleIncreaser[4]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} Retirement modules, for a total increase of ${2}.", moduleId, retirementCount, moduleIncreaser[5]);
        Debug.LogFormat("[Free Parking #{0}] There are {1} Battleship/Semaphore/Maritime Flags/Morse War modules, for a total increase of ${2}.", moduleId, battleFlagSemaphoreMorseCount, moduleIncreaser[6]);
        Debug.LogFormat("[Free Parking #{0}] Adding on the sum of all known modules' values (${1}), the new amount is ${2}.", moduleId, totalIncrease, baseMoneyInt);

        baseMoneyIntOriginal = baseMoneyInt;
        if(baseMoneyInt == 0)
        {
            valueGrounded = true;
            Debug.LogFormat("[Free Parking #{0}] The value has been grounded permanently at $0.", moduleId);
        }
    }

    void Update()
    {
        solvedModulesNames = Bomb.GetSolvedModuleNames();
        if(burglarSillyCheapPresent && !note3Solved)
        {
            if(moduleNames.Where((x) => x.Equals("The Jewel Vault")).Count() == solvedModulesNames.Where((x) => x.Equals("The Jewel Vault")).Count() && moduleNames.Where((x) => x.Equals("Silly Slots")).Count() == solvedModulesNames.Where((x) => x.Equals("Silly Slots")).Count() && moduleNames.Where((x) => x.Equals("Cheap Checkout")).Count() == solvedModulesNames.Where((x) => x.Equals("Cheap Checkout")).Count())
            {
                note3Solved = true;
                baseMoneyInt = 0;
                valueGrounded = true;
                Debug.LogFormat("[Free Parking #{0}] The bomb is now bankrupt. The new amount is $0.", moduleId);
            }
        }
        tempSolvedModules = solvedModules;
        solvedModules = Bomb.GetSolvedModuleNames().Count();
        tempStrikeCount = strikeCount;
        strikeCount = Bomb.GetStrikes();
        if((tempSolvedModules != solvedModules || tempStrikeCount != strikeCount) && !moduleSolved && !note3Solved)
        {
            Debug.LogFormat("[Free Parking #{0}] A module has been solved or a strike has been incurred. The value is being recalculated.", moduleId);
            Recalculate();
        }
    }

    void Recalculate()
    {
        if(valueGrounded)
        {
            Debug.LogFormat("[Free Parking #{0}] The value has been grounded permanently at $0.", moduleId);
            return;
        }
        subtraction = (strikeCount * -200) + (solvedModules * moduleSolveValues[tokenIndex]);
        baseMoneyInt = baseMoneyIntOriginal + subtraction;
        if(baseMoneyInt <= 0)
        {
            baseMoneyInt = 0;
            valueGrounded = true;
            Debug.LogFormat("[Free Parking #{0}] The value has been grounded permanently at $0.", moduleId);
        }
        else
        {
            Debug.LogFormat("[Free Parking #{0}] The total solved modules is now {1}. The total strike count is now {2}. The original base value was ${3}. A total of ${4} has now been added/subtracted. The new value is ${5}.", moduleId, solvedModules, strikeCount, baseMoneyIntOriginal, subtraction, baseMoneyInt);
        }
    }

    public void PressToken()
    {
        if(moduleSolved)
        {
            return;
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        Audio.PlaySoundAtTransform("dice", transform);
        startScreen.SetActive(false);
        mainScreen.SetActive(true);
        surfaceRend.material = backgroundMaterials[1];
    }

    public void PressGo()
    {
        if(moduleSolved)
        {
            return;
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(pressJail)
        {
            Debug.LogFormat("[Free Parking #{0}] Strike! You tried to pass GO! when you should be going to jail.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            FreeParkingEtc();
            return;
        }
        moduloMoney = baseMoneyInt;
        if (baseMoneyInt > 5000)
        {
            baseMoneyInt = baseMoneyInt % 5000;
            Debug.LogFormat("[Free Parking #{0}] The value at submission was greater than $5000. Modulo 5000. The new value is ${1}.", moduleId, baseMoneyInt);
        }
        if(paidMoney == baseMoneyInt)
        {
            Debug.LogFormat("[Free Parking #{0}] You have paid the bomb ${1}. That is correct. Module disarmed.", moduleId, paidMoney);
            finishCard.enabled = true;
            finishCard2.enabled = true;
            if(baseMoneyInt <= 0)
            {
                finishCard.material = finishMatOptions[0];
                Audio.PlaySoundAtTransform("bankrupt", transform);
                finishCard2.enabled = false;
            }
            else
            {
                finishCard2.material = finishMatOptions[2];
                Audio.PlaySoundAtTransform("cash", transform);
                finishCard.enabled = false;
            }
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            FreeParkingEtc();
        }
        else
        {
            Debug.LogFormat("[Free Parking #{0}] Strike! You have tried to pay the bomb ${1}. That is incorrect. I was expecting ${2}. If you moduloed 5000, revert to the original value before recalculating.", moduleId, paidMoney, baseMoneyInt);
            GetComponent<KMBombModule>().HandleStrike();
            baseMoneyInt = moduloMoney;
            FreeParkingEtc();
        }
    }

    public void PressJail()
    {
        if(moduleSolved)
        {
            return;
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(!pressJail)
        {
            Debug.LogFormat("[Free Parking #{0}] Strike! You tried to go to jail. That is not correct.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            FreeParkingEtc();
        }
        else
        {
            Debug.LogFormat("[Free Parking #{0}] You went to jail. Module disarmed.", moduleId);
            finishCard2.enabled = true;
            finishCard2.material = finishMatOptions[1];
            Audio.PlaySoundAtTransform("jailSFX", transform);
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            FreeParkingEtc();
        }

    }

    public void PressFreeParking()
    {
        GetComponent<KMSelectable>().AddInteractionPunch();
        Audio.PlaySoundAtTransform("horn", transform);
        FreeParkingEtc();
    }

    public void FreeParkingEtc()
    {
        foreach(Renderer money in paidMoneyRend)
        {
            money.enabled = false;
        }
        paidMoney = 0;
        moneyStage = 0;
        startScreen.SetActive(true);
        mainScreen.SetActive(false);
        surfaceRend.material = backgroundMaterials[0];
        if(!moduleSolved)
        {
            Debug.LogFormat("[Free Parking #{0}] You have returned to the Free Parking square. Paid money reset to $0.", moduleId);
        }
        else
        {
            surfaceRend.material = backgroundMaterials[2];
            tokenObject.SetActive(false);
        }
    }

    void StartMoneyPress(MoneyInformation pressedMoney)
    {
        if(moduleSolved)
        {
            return;
        }
        GetComponent<KMSelectable>().AddInteractionPunch(0.25f);
        Audio.PlaySoundAtTransform("count", transform);
        if(moneyStage < 21)
        {
            paidMoneyRend[moneyStage].enabled = true;
            paidMoneyRend[moneyStage].material = pressedMoney.material;
            paidMoney += pressedMoney.value;
            moneyStage++;
        }
    }

    // Twitch Plays implementation handled by Kaito Sinclaire (K_S_)
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use '!{0} pay 1234' or '!{0} $1234' to pay the bomb, and '!{0} go to jail' to send it to jail.";
#pragma warning restore 414

    public IEnumerator ProcessTwitchCommand(string command)
    {
        Match mt;

        if (Regex.IsMatch(command, @"^\s*go\s*to\s*jail\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            Debug.LogFormat("[Free Parking #{0}] TP Command: Sending the bomb to jail.", moduleId);

            yield return null;
            yield return new KMSelectable[] { tokenButton, jailButton };
        }
        else if ((mt = Regex.Match(command, @"^\s*(?:pay\s+\$?|\$)(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            // If this matched, group 1 is the amount to pay
            int amountToPay = int.MaxValue;

            // I just know SOME dunce is going to try to pay an inordinately large amount of money.
            // Probably me.
            try { amountToPay = Convert.ToInt32(mt.Groups[1].ToString()); }
            catch (OverflowException e) { /* NO-OP */ }

            if (amountToPay > 5000)
                yield return "sendtochaterror The bank doesn't have enough money to pay that!";
            else if (amountToPay == 0)
            {
                Debug.LogFormat("[Free Parking #{0}] TP Command: Paying the bomb nothing.", moduleId);
                yield return null;
                yield return new KMSelectable[] { tokenButton, goButton };
            }
            else
            {
                List<KMSelectable> pressList = new List<KMSelectable>();

                Debug.LogFormat("[Free Parking #{0}] TP Command: Paying the bomb ${1}.", moduleId, amountToPay);
                pressList.Add(tokenButton);

                // NOTE: This makes the assumption that the money buttons are lowest to highest from left to right.
                for (int i = startMoney.Length - 1; i >= 0; --i)
                {
                    while (amountToPay >= startMoney[i].value)
                    {
                        pressList.Add(startMoney[i].selectable);
                        amountToPay -= startMoney[i].value;
                    }
                    if (amountToPay == 0)
                        break;
                }

                if (amountToPay != 0)
                    yield return "sendtochaterror For some reason, I couldn't pay that amount of money. This is probably a bug.";
                else
                {
                    pressList.Add(goButton);
                    yield return null;

                    KMSelectable[] pressArray = pressList.ToArray();
                    yield return pressArray;
                }
            }
        }
        yield break;
    }
}
