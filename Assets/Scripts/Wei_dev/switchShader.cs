using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchShader : MonoBehaviour
{
    [SerializeField] public Material Toon_Cat;
    [SerializeField] public Animator jumpAnimator;
    [SerializeField] public Animator fadeController;
    [SerializeField] public GameObject fish;
    [SerializeField] public Dictionary<string, int> cat_map = new Dictionary<string, int> 
    { { "Cat.L.012", 0 }, { "Cat.L.012 (1)", 1 }, { "Cat.L.012 (2)", 2 } };

    [SerializeField] private float elapsedTime = 0;
    [SerializeField] private float outElapsedTime = 0;

    private bool isOutBox;
    private Material original_mat;
    

    // Start is called before the first frame update
    void Start()
    {
        isOutBox = false;
        original_mat = GetComponent<SkinnedMeshRenderer>().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (isOutBox)
        {
            outElapsedTime += Time.deltaTime;
            if(outElapsedTime >= 5.0)
            {
                fadeController.SetTrigger("FadeOut");
                CatSelection.CAT_SELECTED = cat_map[transform.parent.name];
            }

        }

        if (!isOutBox)
        {
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
                    meshRenderer.material.SetFloat("_OutlineSize", elapsedTime * 3);
                }

                // Hover for 4 seconds
                if (elapsedTime >= 4.0)
                {
                    // Jump Out Of The Box, Enable Jump Out Animator
                    jumpAnimator.applyRootMotion = false;
                    //transform.parent.GetComponent<BoxCollider>().enabled = true;
                    transform.parent.GetComponent<Animator>().SetTrigger("Jump");
                    isOutBox = true;
                    meshRenderer.material = original_mat;
                }
            }
            else
            {
                meshRenderer.material = original_mat;
                elapsedTime = 0;
            }
        }
    }

    public bool isInRadius(Vector3 pos_a, Vector3 pos_b, float radius)
    {
        if ((pos_b.x - pos_a.x) * (pos_b.x - pos_a.x) + (pos_b.z - pos_a.z) * (pos_b.z - pos_a.z) <= radius * radius)
            return true;

        return false;
    }
}
