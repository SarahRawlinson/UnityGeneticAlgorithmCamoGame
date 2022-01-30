using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA : MonoBehaviour
{
    public float r;
    public float g;
    public float b;
    public float size;
    public bool dead = false;
    public float aliveTime = 0;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;
    public float timeToDie = 0;
    private PopulationManager _populationManager;
    public static event Action Found; 

    private void OnMouseDown()
    {
        dead = true;
        timeToDie = _populationManager.elapsed;
        _spriteRenderer.enabled = false;
        _collider2D.enabled = false;
        Found?.Invoke();
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        _populationManager = FindObjectOfType<PopulationManager>();
        _spriteRenderer.color = new Color(r, g, b);
        this.transform.localScale = new Vector3(size, size, size);
    }

    private void Update()
    {
        if (!dead) aliveTime += Time.deltaTime;
    }
}
