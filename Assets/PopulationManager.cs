using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    [SerializeField] GameObject personPrefab;
    [SerializeField] private int populationSize = 10;
    [SerializeField] private float screenTop = 5.4f;
    [SerializeField] private float screenBottom = -3.4f;
    [SerializeField] private float screenLeft = -9.5f;
    [SerializeField] private float screenRight = 9.5f;
    [Range(0f,1f)]
    [SerializeField] private float mutationChance = 0.02f;
    private readonly List<GameObject> _population = new List<GameObject>();
    public float elapsed = 0;
    [SerializeField] private int trialTime = 10;
    private int _generation = 1;
    private readonly GUIStyle _guiStyle = new GUIStyle();
    private Color _backgroundColour = Color.black;
    private enum Mutations { Swap, Random, Merge}
    private enum DNAGenome { R, G, B, Size}

    private int foundTotal = 0;
    private int foundGeneration = 0;
    private int foundAverage = 0;
    private int leftInScene = 0;
    [SerializeField] float maxSize = 1f;
    [SerializeField] float minSize = 0.1f;


    

    private static Color InvertColor (Color colour) {
        return new Color (1.0f-colour.r, 1.0f-colour.g, 1.0f-colour.b);
    }
    private void OnGUI()
    {
        _guiStyle.fontSize = 50;
        _guiStyle.normal.textColor = InvertColor(_backgroundColour);
        GUI.Label(new Rect(10,10,100,20),$"Generation: {_generation}", _guiStyle);
        GUI.Label(new Rect(10, 65, 100, 20), $"Trial Time: {(int)elapsed}", _guiStyle);
        GUI.Label(new Rect(10, 120, 100, 20), $"Found Total: {foundTotal}", _guiStyle);
        GUI.Label(new Rect(10, 175, 100, 20), $"Found Generation: {foundGeneration}", _guiStyle);
        GUI.Label(new Rect(10, 230, 100, 20), $"Found Average: {foundAverage}", _guiStyle);
        GUI.Label(new Rect(10, 285, 100, 20), $"Left In Scene: {leftInScene}", _guiStyle);
    }

    // Start is called before the first frame update
    void Start()
    {
        leftInScene = populationSize;
        DNA.Found += AddFound;
        for (int i = 0; i < populationSize; i++)
        {
            var go = CreatePerson(out var dna);
            dna.r = Random.Range(0.0f, 1.0f);
            dna.g = Random.Range(0.0f, 1.0f);
            dna.b = Random.Range(0.0f, 1.0f);
            dna.size = Random.Range(minSize, maxSize);
            _population.Add(go);
        }

        DNA dnaBackground = _population[0].GetComponent<DNA>();
        _backgroundColour = new Color(dnaBackground.r, dnaBackground.g, dnaBackground.b);
        FindObjectOfType<Camera>().backgroundColor = _backgroundColour;
    }

    private void AddFound()
    {
        foundTotal++;
        foundGeneration++;
        leftInScene = populationSize - foundGeneration;
    }

    private GameObject CreatePerson(out DNA dna)
    {
        Vector3 pos = new Vector3(Random.Range(screenLeft, screenRight), Random.Range(screenTop, screenBottom), 0);
        GameObject go = Instantiate(personPrefab, pos, Quaternion.identity);
        dna = go.GetComponent<DNA>();
        return go;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > trialTime || leftInScene == 0)
        {
            BreedNewPopulation();
            elapsed = 0f;
            foundGeneration = 0;
            foundAverage = Mathf.RoundToInt((float)foundTotal / _generation);
            leftInScene = populationSize;
        }
    }

    private void BreedNewPopulation()
    {
        List<GameObject> newPopulation = new List<GameObject>();
        List<GameObject> sortedList = _population.OrderBy(o => o.GetComponent<DNA>().timeToDie).ToList();
        _population.Clear();
        for (int i = (int)(sortedList.Count / 2.0f) - 1; i < sortedList.Count - 1; i++)
        {
            _population.Add(Breed(sortedList[i], sortedList[i + 1]));
            _population.Add(Breed(sortedList[i + 1], sortedList[i]));
        }

        for (int i = 0; i < sortedList.Count; i++)
        {
            Destroy(sortedList[i]);
        }

        _generation++;
    }

    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        var go = CreatePerson(out var dna);
        DNA dna1 = parent1.GetComponent<DNA>();
        DNA dna2 = parent2.GetComponent<DNA>();
        dna.r = Random.Range(0, 10) < 5 ? dna1.r : dna2.r;
        dna.g = Random.Range(0, 10) < 5 ? dna1.g : dna2.g;
        dna.b = Random.Range(0, 10) < 5 ? dna1.b : dna2.b;
        dna.size = Random.Range(0, 10) < 5 ? dna1.size : dna2.size;
        DNAGenome randomDnaGenome = RandomRGB();
        switch (randomDnaGenome)
        {
            case DNAGenome.R:
                dna.r = Mutate(dna.r, dna1, dna2,  DNAGenome.R);
                break;
            case DNAGenome.G:
                dna.g = Mutate(dna.g,dna1, dna2, DNAGenome.G);
                break;
            case DNAGenome.B:
                dna.b = Mutate(dna.b,dna1, dna2,  DNAGenome.B);
                break;
            case DNAGenome.Size:
                dna.size = Mutate(dna.size, dna1, dna2,DNAGenome.Size);
                break;
        }
        return go;
    }

    private float Mutate(float original, DNA dna1, DNA dna2, DNAGenome dnaGenome)
    {
        bool mutation = Random.Range(0f, 1f) <= mutationChance;
        if (!mutation) return original;
        var values = Enum.GetValues(typeof(Mutations));
        Mutations mutations = (Mutations) Enum.ToObject(typeof(Mutations), Random.Range(0, values.Length));
        DNAGenome randomDnaGenome = RandomRGB();
        float parent1;
        float parent2;
        (parent1, parent2) = NewMethod(dna1, dna2, dnaGenome);
        switch (mutations)
        {
            case (Mutations.Random):
                return Random.Range(0f, 1f);
            case (Mutations.Swap):
                float ran1;
                float ran2;
                (ran1, ran2) = NewMethod(dna1, dna2, randomDnaGenome);
                return Random.Range(0f, 1f) < .5f ? ran1 : ran2;
            case (Mutations.Merge):
                return (parent1 + parent2) / 2f;
        }

        return original;

    }

    private static DNAGenome RandomRGB()
    {
        var values = Enum.GetValues(typeof(Mutations));
        DNAGenome randomDnaGenome = (DNAGenome) Enum.ToObject(typeof(DNAGenome), Random.Range(0, values.Length));
        return randomDnaGenome;
    }

    private static (float,float) NewMethod(DNA dna1, DNA dna2, DNAGenome dnaGenome)
    {
        float parent1 = 0;
        float parent2 = 0;
        switch (dnaGenome)
        {
            case DNAGenome.R:
                parent1 = dna1.r;
                parent2 = dna2.r;
                break;
            case DNAGenome.G:
                parent1 = dna1.g;
                parent2 = dna2.g;
                break;
            case DNAGenome.B:
                parent1 = dna1.b;
                parent2 = dna2.b;
                break;
            case DNAGenome.Size:
                parent1 = dna1.size;
                parent2 = dna2.size;
                break;
        }

        return (parent1, parent2);
    }
}
