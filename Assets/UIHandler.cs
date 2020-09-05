using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] GameObject _imagePrefab = null;
    Canvas _UIcanvas = null;

    private void Awake()
    {
        _UIcanvas = GetComponent<Canvas>();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
