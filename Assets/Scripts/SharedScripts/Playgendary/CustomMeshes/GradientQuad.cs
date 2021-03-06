﻿using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GradientQuad : MonoBehaviour {

	public enum GradientDirectionEnum {

		Horizontal = 0,
		Vertical = 1,
	}

	public enum AnchorEnum {

		LowerLeft = 0,
		LowerCenter = 1,
		LowerRight = 2,
		MiddleLeft = 3,
		MiddleCenter = 4,
		MiddleRight = 5,
		UpperLeft = 6,
		UpperCenter = 7,
		UpperRight = 8,
	}

	[SerializeField] Mesh mesh = null;
	[SerializeField] AnchorEnum anchor = AnchorEnum.MiddleCenter;
	[SerializeField] Vector2 size = Vector2.zero;
	[SerializeField] Vector2 sizeMultiplier = Vector2.one;
	[SerializeField] GradientDirectionEnum gradientDirection = GradientDirectionEnum.Horizontal;
	[SerializeField] Gradient gradientColors = new Gradient();
	[SerializeField] Vector2 scale = Vector2.one;
	[SerializeField] Vector2 offset = Vector2.zero;
	[SerializeField] bool isUpdate = true;
	Transform cachedTransform;
	MeshRenderer cachedRenderer;
	List<float> portions = new List<float>();

	public AnchorEnum Anchor {
		get { return anchor; }
		set {
			if (anchor != value) {
				anchor = value;
				UpdateMesh();
			}
		}
	}

	public Vector2 Size {
		get { return size; }
		set {
			if (size != value) {
				size = value;
				UpdateMesh();
			}
		}
	}

	public Vector2 SizeMultiplier {
		get { return sizeMultiplier; }
	}

	public GradientDirectionEnum GradientDirection {
		get { return gradientDirection; }
		set {
			if (gradientDirection != value) {
				gradientDirection = value;
				UpdateMesh();
			}
		}
	}

	public Vector2 Scale {
		get { return scale; }
		set {
			if (scale != value) {
				scale = value;
				UpdateMeshRenderer();
				UpdateMesh();
			}
		}
	}

	public Vector2 Offset {
		get { return offset; }
		set {
			if (offset != value) {
				offset = value;
				UpdateOffset();
			}
		}
	}
	
	void Awake() {
		InitMaterial();
		UpdateOffset();
		UpdateMeshRenderer();
		UpdateMesh();
	}

	void OnValidate() {
		if (isUpdate) UpdateMesh();
	}

	Transform CachedTransform {
		get {
			if (cachedTransform == null) {
				cachedTransform = transform;
			}
			return cachedTransform;
		}
	}

	MeshRenderer CachedRenderer {
		get {
			if (cachedRenderer == null) {
				cachedRenderer = GetComponent<MeshRenderer>();
			}
			return cachedRenderer;
		}
	}

	void InitMaterial() {
//        CachedRenderer.material = new Material(CachedRenderer.sharedMaterial);
	}

	void UpdateMeshRenderer() {
//		CachedRenderer.enabled = (Mathf.Min((scale.x * sizeMultiplier.x), (scale.y * sizeMultiplier.y)) > 0.01f) && (Mathf.Min(size.x, size.y) > 0f);
	}

	void UpdateOffset() {
		CachedRenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
	}

	void UpdateMesh() {
		if (mesh == null) {
			mesh = GetComponent<MeshFilter>().sharedMesh;
			if (mesh == null) {
				mesh = new Mesh();
				mesh.hideFlags = HideFlags.DontSave;
				GetComponent<MeshFilter>().mesh = mesh;
			}
		}
		GetComponent<MeshFilter>().sharedMesh = mesh;
		mesh.Clear();
		portions.Clear();
		foreach (GradientColorKey key in gradientColors.colorKeys) {
			if (!portions.Contains(key.time)) portions.Add(key.time);
		}
		foreach (GradientAlphaKey key in gradientColors.alphaKeys) {
			if (!portions.Contains(key.time)) portions.Add(key.time);
		}
		portions.Sort();
		int colorsCount = portions.Count;
		int parts = colorsCount > 2 ? (colorsCount - 1) : 1;
		int vertexCount = parts * 2 + 2;
		Vector3[] meshVertices = new Vector3[vertexCount];
		int[] meshTriangles = new int[parts * 2 * 3];
		Vector2[] meshUV = new Vector2[vertexCount];
		Color[] colors = new Color[vertexCount];
		Vector2 halfSize = size * 0.5f;
		Vector2 firstSize = Vector2.zero;
		Vector2 secondSize = Vector2.zero;
		Vector2 anchorOffset = Vector2.zero;
		switch (anchor) {
		case AnchorEnum.LowerLeft:
		case AnchorEnum.LowerCenter:
		case AnchorEnum.LowerRight:
			firstSize[(int) gradientDirection] = size[(int) gradientDirection];
			secondSize = size;
			anchorOffset.x = -halfSize.x * (int) anchor;
			break;
		case AnchorEnum.MiddleLeft:
		case AnchorEnum.MiddleCenter:
		case AnchorEnum.MiddleRight:
			if (gradientDirection == GradientDirectionEnum.Horizontal) {
				firstSize.Set(size.x, -halfSize.y);
				secondSize.Set(size.x, halfSize.y);
				anchorOffset.x = -halfSize.x * ((int) anchor - 3);
			} else
			if (gradientDirection == GradientDirectionEnum.Vertical) {
				firstSize.y = size.y;
				secondSize = size;
				anchorOffset.Set((-halfSize.x * ((int) anchor - 3)), -halfSize.y);
			}
			break;
		case AnchorEnum.UpperLeft:
		case AnchorEnum.UpperCenter:
		case AnchorEnum.UpperRight:
			if (gradientDirection == GradientDirectionEnum.Horizontal) {
				firstSize.Set(size.x, -size.y);
				secondSize.x = size.x;
				anchorOffset.x = -halfSize.x * ((int) anchor - 6);
			} else
			if (gradientDirection == GradientDirectionEnum.Vertical) {
				firstSize.y = size.y;
				secondSize = size;
				anchorOffset.Set((-halfSize.x * ((int) anchor - 6)), -size.y);
			}
			break;
		}
		Vector3 newScale = CachedTransform.localScale;
		newScale.x = scale.x * sizeMultiplier.x;
		newScale.y = scale.y * sizeMultiplier.y;
		CachedTransform.localScale = newScale;
		for (int i = 0; i < (vertexCount / 2); i++) {
			if (gradientDirection == GradientDirectionEnum.Horizontal) {
				meshVertices[2 * i] = new Vector3((anchorOffset.x + firstSize.x * portions[i]), (anchorOffset.y + firstSize.y));
				meshVertices[2 * i + 1] = new Vector3((anchorOffset.x + secondSize.x * portions[i]), (anchorOffset.y + secondSize.y));
				meshUV[2 * i] = new Vector2(portions[i], 0f);
				meshUV[2 * i + 1] = new Vector2(portions[i], 1f);
			} else
			if (gradientDirection == GradientDirectionEnum.Vertical) {
				meshVertices[2 * i] = new Vector3((anchorOffset.x + firstSize.x), (anchorOffset.y + firstSize.y * portions[i]));
				meshVertices[2 * i + 1] = new Vector3((anchorOffset.x + secondSize.x), (anchorOffset.y + secondSize.y * portions[i]));
				meshUV[2 * i] = new Vector2(0f, portions[i]);
				meshUV[2 * i + 1] = new Vector2(1f, portions[i]);
			}
			meshUV[2 * i].Scale(newScale);
			meshUV[2 * i + 1].Scale(newScale);
			colors[2 * i] = colors[2 * i + 1] = gradientColors.Evaluate(portions[i]);
		}
		for (int i = 0; i < parts; i++) {
			meshTriangles[6 * i + 0] = 2 * i;
			meshTriangles[6 * i + 1] = 2 * i + 1;
			meshTriangles[6 * i + 2] = 2 * i + 2;
			meshTriangles[6 * i + 3] = 2 * i + 1;
			meshTriangles[6 * i + 4] = 2 * i + 3;
			meshTriangles[6 * i + 5] = 2 * i + 2;
		}
		mesh.vertices = meshVertices;
		mesh.triangles = meshTriangles;
		mesh.uv = meshUV;
		mesh.colors = colors;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
        #if !UNITY_5_5_OR_NEWER
        mesh.Optimize();
        #endif
	}
}
