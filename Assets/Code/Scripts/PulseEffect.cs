using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private float scaleAmount = 0.1f;
    [SerializeField] private float scaleSpeed = 2f;

    [Header("Emission")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float emissionStrength = 2f;
    [SerializeField] private float emissionSpeed = 2f;

    private Vector3 startScale;
    private Material mat;

    private void Start()
    {
        startScale = transform.localScale;

        if (sr != null)
        {
            mat = sr.material;
        }
    }

    private void Update()
    {
        float pulse = Mathf.Sin(Time.time * scaleSpeed);


        transform.localScale = startScale + Vector3.one * pulse * scaleAmount;


        if (mat != null)
        {
            float emission = (pulse + 1f) / 2f * emissionStrength;
            mat.SetColor("_EmissionColor", Color.magenta * emission);
        }
    }
}