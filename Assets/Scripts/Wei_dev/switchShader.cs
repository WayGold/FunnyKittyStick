using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchShader : MonoBehaviour
{
    [SerializeField] public Material Toon_Cat;
    [SerializeField] public GameObject fish;
    [SerializeField] private float elapsedTime = 0;

    private Material original_mat;

    // Start is called before the first frame update
    void Start()
    {
        original_mat = GetComponent<SkinnedMeshRenderer>().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        if (isInRadius(transform.position, fish.transform.position, 5))
        { 
            if (elapsedTime == 0)
            {
                elapsedTime += Time.deltaTime;
                original_mat = meshRenderer.material;
                meshRenderer.material = Toon_Cat;
            }
            else
            {
                elapsedTime += Time.deltaTime;
                meshRenderer.material.SetFloat("_OutlineSize", elapsedTime * 2);
            }

            // Hover for 2 seconds
            if(elapsedTime >= 4.0)
            {
                // Jump Out Of The Box, Enable Jump Out Animator
                transform.parent.GetComponent<Animator>().enabled = true;
                meshRenderer.material = original_mat;
            }
        }
        else
        {
            meshRenderer.material = original_mat;
            elapsedTime = 0;
        }
    }

    public bool isInRadius(Vector3 pos_a, Vector3 pos_b, float radius)
    {
        if ((pos_b.x - pos_a.x) * (pos_b.x - pos_a.x) + (pos_b.z - pos_a.z) * (pos_b.z - pos_a.z) <= radius * radius)
            return true;

        return false;
    }
}
