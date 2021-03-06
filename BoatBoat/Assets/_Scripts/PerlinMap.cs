﻿// TEAM:
// Mighty Morphin Pingas Rangers
// Sebastian Monroy - sebash@gatech.edu - smonroy3
// Thomas Cole Carver - tcarver3@gatech.edu - tcarver3
// Chase Johnston - cjohnston8@gatech.edu - cjohnston8
// Jory Folker - jfolker10@outlook.com - jfolker3
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerlinMap : MonoBehaviour {
	public static PerlinMap instance;
	
	public Color lowColor, highColor;
	public float colorExponent;
	public float pixelScale;
	public float timeScale;
	public float heightScale;
	private float xOffset, yOffset;

	public int meshSize;
	private int texSize;
	private Texture2D baseTex;

	public Material verticesMaterial;
	public MeshFilter terrainMesh;
	private Vector3[] verts;
	private List<int> tris;
	private Vector2[] uvs;
	private Color[] colors;
	private float[] heights;

	public bool whirlpoolsEnabled;
	public GameObject[] allWhirlpools;

	private float blockSize, pixelSize;
	private bool[] render;

	// Use this for initialization
	void Start () {
		texSize = meshSize / 4;
		baseTex = new Texture2D(texSize, texSize);

		verts = new Vector3[meshSize*meshSize];
		tris = new List<int>();
		uvs = new Vector2[meshSize*meshSize];
		colors = new Color[baseTex.width*baseTex.height];
		heights = new float[meshSize*meshSize];
		render = new bool[meshSize*meshSize];

		this.renderer.enabled = true;

		//this.gameObject.renderer.material.SetTextureScale("_MainTex",new Vector2(1/this.transform.localScale.x,1/this.transform.localScale.z));
		//renderer.material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));

		blockSize = this.transform.localScale.x / meshSize;
		pixelSize = this.transform.localScale.x / texSize;
		for (int j = 0; j < meshSize; j++) {
			float zCoord = this.transform.position.z - this.transform.localScale.z/2 + j*blockSize + blockSize/2;
			for (int i = 0; i < meshSize; i++) {
				float xCoord = this.transform.position.x - this.transform.localScale.x/2 + i*blockSize + blockSize/2;

				if (isWithinRadius(xCoord, zCoord)) {
					render[j * meshSize + i] = true;
					verts[j * meshSize + i] = new Vector3(xCoord/this.transform.localScale.x, this.transform.position.y, zCoord/this.transform.localScale.z);
					uvs[j * meshSize + i] = new Vector2(xCoord, zCoord);
					//colors[j * meshSize + i] = lowColor;

					//if (i+1 >= meshSize || j+1 >= meshSize || !isWithinRadius(i+1,j) || !isWithinRadius(i,j+1)) {
					if (!isWithinRadius(i+1,j) || !isWithinRadius(i,j+1) || !isWithinRadius(i+1,j+1)) {
						continue;
					}
					tris.Add(i + j*meshSize);
					tris.Add(i + (j+1)*meshSize);
					tris.Add((i+1) + j*meshSize);

					tris.Add(i + (j+1)*meshSize);
					tris.Add((i+1) + (j+1)*meshSize);
					tris.Add((i+1) + j*meshSize);
				}
			}
		}

		Mesh ret = new Mesh();
        ret.vertices = verts;
        ret.triangles = tris.ToArray();
        ret.uv = uvs;

        ret.RecalculateBounds();
        ret.RecalculateNormals();
        terrainMesh.mesh = ret;

        /*baseTex.SetPixels(colors);
		baseTex.Apply();
        renderer.material.SetTexture("_MainTex", baseTex);*/

		// xOffset = -this.transform.position.x;
		// yOffset = -this.transform.position.y;
		// allWhirlpools = GameObject.FindGameObjectsWithTag("Whirlpool");
		// if (!whirlpoolsEnabled) {
		// 	DestroyAllWhirlpools();
		// }
	}
	
	// Update is called once per frame
	void Update () {
		CalculateNoise();
	}

	public void CalculateNoise() {
		Vector3 vertPosition;
		for (int j = 0; j < meshSize; j++) {
			float zCoord = this.transform.position.z - this.transform.localScale.z/2 + j*blockSize + blockSize/2;
			for (int i = 0; i < meshSize; i++) {
				float xCoord = this.transform.position.x - this.transform.localScale.x/2 + i*blockSize + blockSize/2;
				
				if (i % 4 == 0 && j % 4 == 0) {	
					colors[j/4 * texSize + i/4] = GetColor(xCoord, zCoord);
					// if there is a vertex at this location, move it and stuff
					
				}

				if (render[j * meshSize + i]) {
					heights[j * meshSize + i] = GetHeight(xCoord, zCoord);//this.transform.position.j + perlin * heightScale;

					vertPosition = verts[j * meshSize + i];
					verts[j * meshSize + i] = new Vector3(vertPosition.x, heights[j * meshSize + i], vertPosition.z);
				}

				// but still get texture colors even if there isn't a vertex here
				
			}
		}

		baseTex.SetPixels(colors);
		baseTex.Apply();
		//renderer.material.SetTexture("_MainTex", baseTex);
		//renderer.material.SetTexture("_Ramp", baseTex);
		renderer.material.SetTexture("_BumpMap", GetNormalMap(baseTex, 1f));

		Mesh ret = terrainMesh.mesh;
        ret.vertices = verts;
        //ret.colors = colors;

        ret.RecalculateBounds();
        ret.RecalculateNormals();
        terrainMesh.mesh = ret;
	}

	private Texture2D GetNormalMap(Texture2D source, float strength) {
         strength=Mathf.Clamp(strength,0.0F,10.0F);
         Texture2D result;
         float xLeft;
         float xRight;
         float yUp;
         float yDown;
         float yDelta;
         float xDelta;
         result = new Texture2D (source.width, source.height, TextureFormat.ARGB32, true);
         for (int by=0; by<result.height; by++) {
                    for (int bx=0; bx<result.width; bx++) {
                                xLeft = source.GetPixel(bx-1,by).grayscale*strength;
                                xRight = source.GetPixel(bx+1,by).grayscale*strength;
                                yUp = source.GetPixel(bx,by-1).grayscale*strength;
                                yDown = source.GetPixel(bx,by+1).grayscale*strength;
                                xDelta = ((xLeft-xRight)+1)*0.5f;
                                yDelta = ((yUp-yDown)+1)*0.5f;
                                result.SetPixel(bx,by,new Color(xDelta,yDelta,1.0f,yDelta));
                    }
         }
         result.Apply();
         return result;
	}

	public bool isWithinRadius(float x, float z) {
		Vector2 center = new Vector2(this.transform.position.x, this.transform.position.z);
		Vector2 point = new Vector2(x, z);

		return (Vector2.Distance(center, point) <= this.transform.lossyScale.x/2);
	}

	public bool isWithinRadius(int i, int j) {
		float xCoord = this.transform.position.x - this.transform.localScale.x/2 + i*blockSize + blockSize/2;
		float zCoord = this.transform.position.z - this.transform.localScale.z/2 + j*blockSize + blockSize/2;

		return isWithinRadius(xCoord, zCoord);
	}

	public float GetVertexValue(float x, float z) {
		if (HasWhirlpool(x, z)) {
			return GetWhirlpoolValue(x, z);
		} else {
			return GetPerlinValue(x, z);
		}
	}

	public float GetPerlinValue(float x, float z) {
		// input in terms of world position
		float perlin1 = -0.5f + Mathf.PerlinNoise(timeScale * Time.time + x * pixelScale + xOffset, timeScale * Time.time + z * pixelScale + yOffset);
		float perlin2 = -0.5f + Mathf.PerlinNoise(-timeScale * Time.time + x * pixelScale + xOffset, -timeScale * Time.time + z * pixelScale + yOffset);
		float value = (0.5f*perlin1 + 0.5f*perlin2);

		return value;
	}

	/*public Color GetColor(int i, int j) {
		// input in terms of array position
		float xCoord = this.transform.position.x - this.transform.localScale.x/2 + i*pixelSize/2 + pixelSize/4;
		float zCoord = this.transform.position.z - this.transform.localScale.z/2 + j*pixelSize/2 + pixelSize/4;

	}*/

	public Color GetColor(float x, float z) {
		// input in terms of world position
		float value = GetVertexValue(x, z);
		Color newColor = lowColor * (1-Mathf.Pow(value+0.5f,colorExponent)) + highColor * (Mathf.Pow(value+0.5f,colorExponent));
	
		return newColor;
	}

	public float GetHeight(float x, float z) {
		// input in terms of world position
		float value = GetVertexValue(x, z);
		if (HasWhirlpool(x, z)) {
			value = value-0.5f;
		}
		float height = this.transform.position.y + value * heightScale;

		return height;
	}

	public float GetWhirlpoolValue(float x, float z) {
		// input in terms of world position
		if (whirlpoolsEnabled) {
			foreach (GameObject whirlpool in allWhirlpools) {
				Vector2 whirlpoolCenter = new Vector2(whirlpool.transform.position.x, whirlpool.transform.position.z);
				float whirlRadius = whirlpool.transform.lossyScale.x*4/7; 

				float dist = Vector2.Distance(whirlpoolCenter, new Vector2(x, z));
				if (dist <= whirlRadius) {
					float value = Mathf.Cos((whirlRadius - dist)/whirlRadius * Mathf.PI)/2;
					return value;
				}
			}
		}

		return -999f;
	}

	public bool HasWhirlpool(float x, float z) {
		// input in terms of world position
		if (whirlpoolsEnabled) {
			foreach (GameObject whirlpool in allWhirlpools) {
				Vector2 whirlpoolCenter = new Vector2(whirlpool.transform.position.x, whirlpool.transform.position.z);
				float whirlRadius = whirlpool.transform.lossyScale.x*4/7; 

				float dist = Vector2.Distance(whirlpoolCenter, new Vector2(x, z));
				if (dist <= whirlRadius) {
					return true;
				}
			}
		}

		return false;
	}

	/*public void CreateWhirlpool(float xWhirl, float zWhirl, float radius) {
		float value, newHeight;
		Color newColor;
		Vector3 vertPosition;

		blockSize = this.transform.localScale.x / texSize;
		whirlCenter = new Vector2(xWhirl, zWhirl);
		whirlRadius = radius;
		for (int j = 0; j < meshSize; j++) {
			for (int i = 0; i < meshSize; i++) {
				if (dist < radius) {
					whirl[j * meshSize + i] = true;

					value = Mathf.Cos((radius - dist)/radius * Mathf.PI);
					newColor =  lowColor * (1-Mathf.Pow(value/2+0.5f,colorExponent)) + highColor * (Mathf.Pow(value/2+0.5f,colorExponent));
					colors[j * meshSize + i] = newColor;

					newHeight =  this.transform.position.y + (value-1)/2 * heightScale;
					heights[j * meshSize + i] = newHeight;

					vertPosition = verts[j * meshSize + i];
					verts[j * meshSize + i] = new Vector3(vertPosition.x, newHeight, vertPosition.z);
				}
			}
		}

		baseTex.SetPixels(colors);
		baseTex.Apply();
		renderer.material.SetTexture("_MainTex", baseTex);

		Mesh ret = terrainMesh.mesh;
        ret.vertices = verts;
        ret.colors = colors;

        ret.RecalculateBounds();
        ret.RecalculateNormals();
        terrainMesh.mesh = ret;
	}*/

	public void DestroyAllWhirlpools() {
		foreach (GameObject whirlpool in allWhirlpools) {
			Destroy(whirlpool);
		}
	}

	// public float GetHeight(float x, float y) {
	// 	//Debug.Log(x + " " + y);
	// 	int newY = (int) (y + this.transform.localScale.z/2);
	// 	int newX = (int) (x + this.transform.localScale.x/2);
	// 	newY = (int) Mathf.Round(newY / blockSize);
	// 	newX = (int) Mathf.Round(newX / blockSize);
	// 	//Debug.Log(newX + "/" + newY);

	// 	return heights[newY * meshSize + newX];
	// }
}
