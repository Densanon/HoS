[System.Serializable]
public class ResourceData 
{
    public string itemName { get; private set; }
    public string displayName { get; private set; }
    public string discription { get; private set; }
    public string gameElementType { get; private set; }
    public string groups { get; private set; }
    public bool visible { get; private set; }
    public int currentAmount { get; private set; }
    public int autoAmount { get; private set; }
    public float craftTime { get; private set; }
    public string consumableRequirements { get; private set; }
    public string nonConsumableRequirements { get; private set; }
    public string itemsToGain { get; private set; }
    public string commandsOnPressed { get; private set; }
    public string commandsOnCreated { get; private set; }
    public string imageName { get; private set; }
    public string soundName { get; private set; }
    public string achievement { get; private set; }
    public int atMostAmount { get; private set; }

    public ResourceData(string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft, 
        string created, string coms, string createComs, string im, string snd, string ach, int most)
    {
        itemName = name;
        displayName = display;
        discription = dis;
        groups = gr;
        consumableRequirements = reqs;
        nonConsumableRequirements = nonReqs;
        visible = vis;
        currentAmount = cur;
        autoAmount = autoA;
        craftTime = craft;
        itemsToGain = created;
        commandsOnPressed = coms;
        commandsOnCreated = createComs;
        imageName = im;
        soundName = snd;
        achievement = ach;
        atMostAmount = most;
    }

    public ResourceData(ResourceData data)
    {
        itemName = data.itemName;
        displayName = data.displayName;
        discription = data.discription;
        groups = data.groups;
        gameElementType = data.gameElementType;
        consumableRequirements = data.consumableRequirements;
        nonConsumableRequirements = data.nonConsumableRequirements;
        visible = data.visible;
        currentAmount = data.currentAmount;
        autoAmount = data.autoAmount;
        craftTime = data.craftTime;
        itemsToGain = data.itemsToGain;
        commandsOnPressed = data.commandsOnPressed;
        commandsOnCreated = data.commandsOnCreated;
        imageName = data.imageName;
        soundName = data.soundName;
        achievement = data.achievement;
        atMostAmount = data.atMostAmount;
    }

    public void SetAtMost(int most)
    {
        atMostAmount = most;
    }

    public void SetCurrentAmount(int amount)
    {
        currentAmount = amount;
    }

    public void AdjustCurrentAmount(int amount)
    {
        currentAmount += amount;
    }

    public void AdjustAutoAmount(int amount)
    {
        autoAmount += amount;
    }

    public void SetCraftTimer(float amount)
    {
        craftTime = amount;
    }

    public void AdjustCraftTimer(float amount)
    {
        craftTime += amount;
    }

    public void AdjustVisibility(bool vis)
    {
        visible = vis;
    }

    public string DigitizeForSerialization()
    {
        return $"{itemName},{displayName},{discription},{groups},{consumableRequirements},{nonConsumableRequirements},{visible},{currentAmount},{autoAmount}," +
            $"{craftTime},{itemsToGain},{commandsOnPressed},{commandsOnCreated},{imageName},{soundName},{achievement};";
    }
}
