using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Steelpusher : MonoBehaviour
{
    public GameObject coin_prefab;
    public float speed = 10;

    //each coin has an associated line renderer
    private List<GameObject> all_coins = new List<GameObject>();
    private List<LineRenderer> line_renderers = new List<LineRenderer>();
    private List<int> selected_coin_indicies = new List<int>();


    private Gradient unselected_gradient = new Gradient();
    private Gradient selected_gradient = new Gradient();

    // Start is called before the first frame update
    void Start()
    {
        float alpha = 0.75f;
        unselected_gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.2f), new GradientAlphaKey(alpha, 1.0f) }
        );
        selected_gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.2f), new GradientAlphaKey(alpha, 1.0f) }
        );
    }

    void SpawnCoin()
    {
        GameObject p = Instantiate(coin_prefab, transform.position, transform.rotation);
        p.GetComponent<Rigidbody>().velocity = transform.forward * speed;
        all_coins.Add(p);

        LineRenderer lineRenderer = p.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.colorGradient = unselected_gradient;
        line_renderers.Add(lineRenderer);
    }

    List<int> get_coins_in_frustrum(float degrees)
    {
        List<int> toRet = new List<int>();
        for (int i = 0; i < all_coins.Count; i++)
        {
            Vector3 targetDir = all_coins[i].transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);
            if (angle <= degrees)
            {
                toRet.Add(i);
            }
        }
        return toRet;
    }

    // Update is called once per frame
    void Update()
    {
        //spawn coins
        if (Input.GetKeyDown("c"))
        {
            SpawnCoin();
        }

        //determine all coins in N degree cone
        List<int> coins_in_frustrum = get_coins_in_frustrum(15.0f);

        //decide how to update selection state
        if (Input.GetKey("g"))
        {
            //add all coins to selection
            selected_coin_indicies.AddRange(coins_in_frustrum);
        }
        if (Input.GetKey("h"))
        {
            //remove all coins from selection
            selected_coin_indicies.RemoveAll(x => coins_in_frustrum.Contains(x));
        }
        if (Input.GetKey("j"))
        {
            //clear selection
            selected_coin_indicies.Clear();
        }

        //update line renderer positions and colors
        Vector3 self_ray_source = transform.position - new Vector3(0.0f, 0.5f, 0.0f);
        for (int i = 0; i < line_renderers.Count; i++)
        {
            line_renderers[i].SetPosition(0, self_ray_source);
            line_renderers[i].SetPosition(1, all_coins[i].transform.position);
            line_renderers[i].colorGradient = selected_coin_indicies.Contains(i) ? selected_gradient : unselected_gradient;
        }

        //todo: resolve physics of selection
        //concept of burn level
    }
}
