using UnityEngine;

public class Rotator : MonoBehaviour
{
    void Update()
    {
        // ¨C¬í±ÛÂà (X, Y, Z) «×
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}