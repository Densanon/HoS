using System;
using System.Collections.Generic;

class WeightedRandomBag<T>
{

    private struct Entry
    {
        public double accumulatedWeight;
        public T item;
    }

    private List<Entry> buildList = new List<Entry>();
    private Entry[] entries;
    private double accumulatedWeight;
    private Random rand = new Random();

    public void AddEntry(T item, double weight)
    {
        accumulatedWeight += weight;
        buildList.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
    }

    public void BuildBag()
    {
        entries = buildList.ToArray();
    }

    public T GetRandom()
    {
        double r = rand.NextDouble() * accumulatedWeight;

        foreach (Entry entry in entries)
        {
            if (entry.accumulatedWeight >= r)
            {
                return entry.item;
            }
        }
        return default(T); //should only happen when there are no entries
    }
}
