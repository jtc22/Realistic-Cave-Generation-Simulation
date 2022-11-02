using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{

    // TODO getlayer()
    // TODO edit functions
    // TODO material class
    // TODO map parameters (rainfall, temp, elevation, humidity, air pressure/makeup), water table
    // TODO pack into CSV file
    // TODO load map from CSV
    // TODO getAttributes(enum) for material 

    // TODO Generation - map features

    public enum Material {
        basalt,
        limestone,
        sandstone,
        dolomite,
        granite,
        water,
        air
    }

    public struct MaterialProperty {
        public string name;
        public float density;
        public float hardness;
        public float solubility;
        public Color color;
    }

    // Public Variables
    public int[,,] mapMatrix { get; set; }
    public int width { get; }
    public int height { get; }
    public int depth { get; }
    public Texture2D[,] materialTextureLayers { get; set; }
    public int waterLevel { get; set; }
    public int waterDepth { get; set; }

    // Private variables
    private float noiseFrequency = 1.01f;
    

    // Constructor
    public Map(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.noiseFrequency = height / Random.Range(25.0f, 55.0f);
        waterLevel = height / 3;
        waterDepth = height / 6;
        mapMatrix = new int[width, height, depth];
        InitializeMap();
    }

    // This where all the cool stuff happens eventually
    void InitializeMap()
    {
        // Create all of the textures needed for the map
        materialTextureLayers = new Texture2D[depth, numMaterials()];
        for (int z = 0; z < depth; z++)
        {
            for (int mat = 0; mat < numMaterials(); mat++)
            {
                Texture2D tempTex = new Texture2D(width, height);

                // Reset all pixels color to transparent
                Color32 resetColor = new Color32(255, 255, 255, 0);
                Color32[] resetColorArray = tempTex.GetPixels32();

                for (int i = 0; i < resetColorArray.Length; i++)
                {
                    resetColorArray[i] = resetColor;
                }

                tempTex.SetPixels32(resetColorArray);

                tempTex.filterMode = FilterMode.Point;
                materialTextureLayers[z, mat] = tempTex;
            }
        }

        float cliffOffset = 0;

        // Loop through all points on the map and set the texture value at that map
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float divisor = height; //Mathf.Max(width, height, depth);
                    float noise = Mathf.Abs(perlinNoise.get3DPerlinNoise(new Vector3((float)x / (width * 10), (float)y / height, (float)z / (width*10)), noiseFrequency));
                    Material material = (Material)(int)(noise * (numMaterials()+1));

                    if(y > (sig(0.1f, 200, x + (int)cliffOffset) * height * 0.9))
                    {
                        material = Material.air;
                    }

                    // Set the air to water if its under the water level
                    if(y <= waterLevel && material == Material.air)
                    {
                        material = Material.water;
                    }

                    MaterialProperty mat = getMaterialProperties(material);
                    mapMatrix[x, y, z] = (int)material;
                    materialTextureLayers[z, (int)material].SetPixel(x, y, mat.color);
                }
                cliffOffset += Random.Range(-1.5f,1.5f);
            }
        }

        // Apply the pixel updates to all textures
        for (int z = 0; z < depth; z++)
        {
            for (int mat = 0; mat < numMaterials(); mat++)
            {
                materialTextureLayers[z, mat].Apply();
            }
        }
    }

    public int numMaterials()
    {
        return System.Enum.GetNames(typeof(Material)).Length;
    }

    public static MaterialProperty getMaterialProperties(Material material){
        MaterialProperty prop = new MaterialProperty();
        switch (material)
        {
            case (Material.basalt):
                prop.name = "Basalt";
                prop.density = 3f;
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(.380f, .412f, .424f, 1);
                return prop;
            case (Material.limestone):
                prop.name = "Limestone";
                prop.density = 2f; // 1.5-2.71 g/cm^3
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(0.74f, 0.74f, 0.56f, 1);
                return prop;
            case (Material.sandstone):
                prop.name = "Sandstone";
                prop.density = 2.3f; // 2.0-2.6
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(.698f, .565f, .510f, 1);
                return prop;
            case (Material.dolomite):
                prop.name = "Dolomite";
                prop.density = 2.84f; // Avg
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(.714f, .702f, .671f, 1);
                return prop;
            case (Material.granite):
                prop.name = "Granite";
                prop.density = 2.75f; // 2.7-2.8
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(.404f, .404f, .404f, 1);
                return prop;
            case (Material.water):
                prop.name = "Water";
                prop.density = 1.0f;
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(0.1f, 0.1f, 0.8f, 1);
                return prop;
            case (Material.air):
                prop.name = "Air";
                prop.density = 1f;
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(0.9f, 0.9f, 1.0f, 1);
                return prop;
            default:
                prop.name = "nothing";
                prop.density = 1f;
                prop.hardness = 1;
                prop.solubility = 1;
                prop.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                return prop;
        }
    }

    float sig(float c1, int c2, int x)
    {
        return 1/(1 + Mathf.Exp(-1 * c1 * (x - c2)));
    }

}
