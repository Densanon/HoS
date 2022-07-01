[System.Serializable]
public class ResourceData 
{
    public string itemName { get; private set; }
    public string displayName { get; private set; }
    public string description { get; private set; }
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
    public string buildables { get; private set; }

    public ResourceData(string name, string display, string des, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft, 
        string created, string coms, string createComs, string im, string snd, string ach, int most, string build)
    {
        itemName = name;
        displayName = display;
        description = des;
        groups = gr;
        gameElementType = eType;
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
        buildables = build;
    }

    public ResourceData(ResourceData data)
    {
        itemName = data.itemName;
        displayName = data.displayName;
        description = data.description;
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
        buildables = data.buildables;
    }

    public void SetDisplayName(string name)
    {
        displayName = name;

    }

    public void SetDescription(string des)
    {
        description = des;
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

    public void SetAutoAmount(int amount)
    {
        autoAmount = amount;
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

    public void SetBuildablesString(string build)
    {
        buildables = build;
    }

    public void SetGroups(string group)
    {
        groups = group;
    }

    public void SetGameElementType(string type)
    {
        gameElementType = type;

    }

    public void SetConsumableRequirements(string reqs)
    {
        consumableRequirements = reqs;
    }

    public void SetNonConsumableRequirements(string reqs)
    {
        nonConsumableRequirements = reqs;
    }

    public void SetItemsToGain(string items)
    {
        itemsToGain = items;
    }

    public void SetCommandsOnPressed(string coms)
    {
        commandsOnPressed = coms;
    }

    public void SetCommandsOnCreated(string coms)
    {
        commandsOnCreated = coms;
    }

    public void SetImageName(string name)
    {
        imageName = name;
    }

    public void SetSoundName(string name)
    {
        soundName = name;
    }

    public void SetAchievementName(string name)
    {
        achievement = name;
    }

    public string DigitizeForSerialization()
    {
        //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
        //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
        return $"{itemName},{displayName},{description},{groups},{gameElementType},{consumableRequirements},{nonConsumableRequirements},{visible},{currentAmount},{autoAmount}," +
            $"{craftTime},{itemsToGain},{commandsOnPressed},{commandsOnCreated},{imageName},{soundName},{achievement},{atMostAmount},{buildables};";
    }
}
