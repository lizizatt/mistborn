using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Steelpusher : MonoBehaviour
{
    public Rigidbody own_body;
    public GameObject coin_prefab;
    public float speed = 10;

    //each coin has an associated line renderer
    private List<GameObject> all_coins = new List<GameObject>();
    private List<LineRenderer> line_renderers = new List<LineRenderer>();


    private List<int> selected_push_coin_indicies = new List<int>();
    private List<int> selected_pull_coin_indicies = new List<int>();

    private Gradient unselected_gradient = new Gradient();
    private Gradient selected_push_gradient = new Gradient();
    private Gradient selected_pull_gradient = new Gradient();

    // Start is called before the first frame update
    void Start()
    {
        float alpha = 0.75f;
        unselected_gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.2f), new GradientAlphaKey(alpha, 1.0f) }
        );
        selected_push_gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.2f), new GradientAlphaKey(alpha, 1.0f) }
        );
        selected_pull_gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.white, 1.0f) },
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
            Vector3 target_dir = all_coins[i].transform.position - transform.position;
            float angle = Vector3.Angle(target_dir, transform.forward);
            if (angle <= degrees)
            {
                toRet.Add(i);
            }
        }
        return toRet;
    }

    void ResolveForces(float push_level, float pull_level) 
    {
        float push_force = 10.0f * push_level;
        float pull_force = 10.0f * pull_level;

        for (int i = 0; i < all_coins.Count; i++) {
            if (selected_push_coin_indicies.Contains(i)) {
                Vector3 target_dir = (all_coins[i].transform.position - transform.position).normalized;
                RaycastHit info;
                Rigidbody coin_body = all_coins[i].GetComponent<Rigidbody>();
                if (coin_body.SweepTest(target_dir, out info, 0.25f)) {
                    own_body.AddForce(-1.0f * push_force * target_dir);
                } else {
                    coin_body.AddForce(push_force * target_dir);
                }
            } else if (selected_pull_coin_indicies.Contains(i)) {
                Vector3 target_dir = (all_coins[i].transform.position - transform.position).normalized;
                RaycastHit info;
                Rigidbody coin_body = all_coins[i].GetComponent<Rigidbody>();
                if (coin_body.SweepTest(-1.0f * target_dir, out info, 0.25f)) {
                    own_body.AddForce(pull_force * target_dir);
                } else {
                    coin_body.AddForce(-1.0f * pull_force * target_dir);
                }
            }
        }

        //add new push forces this frame
        for (int i = 0; i < selected_push_coin_indicies.Count; i++) {
            int coin_ind = selected_push_coin_indicies[i];
        }
        //add new pull forces this frame
        for (int i = 0; i < selected_pull_coin_indicies.Count; i++) {
            int coin_ind = selected_pull_coin_indicies[i];
            Vector3 target_dir = (all_coins[coin_ind].transform.position - transform.position).normalized;
            all_coins[coin_ind].GetComponent<Rigidbody>().AddForce(-1.0f * target_dir * pull_force);
        }
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

        //update selection state for push coins
        if (Input.GetKey("r"))
        {
            //add all coins to selection
            selected_push_coin_indicies.AddRange(coins_in_frustrum);
            selected_pull_coin_indicies.RemoveAll(x => coins_in_frustrum.Contains(x));
        }
        if (Input.GetKey("t"))
        {
            //remove all coins from selection
            selected_push_coin_indicies.RemoveAll(x => coins_in_frustrum.Contains(x));
        }
        if (Input.GetKey("y"))
        {
            //clear selection
            selected_push_coin_indicies.Clear();
        }

        //update selection state for pull coins
        if (Input.GetKey("f"))
        {
            //add all coins to selection
            selected_pull_coin_indicies.AddRange(coins_in_frustrum);
            selected_push_coin_indicies.RemoveAll(x => coins_in_frustrum.Contains(x));
        }
        if (Input.GetKey("g"))
        {
            //remove all coins from selection
            selected_pull_coin_indicies.RemoveAll(x => coins_in_frustrum.Contains(x));
        }
        if (Input.GetKey("h"))
        {
            //clear selection
            selected_pull_coin_indicies.Clear();
        }

        //update line renderer positions and colors
        Vector3 self_ray_source = transform.position - new Vector3(0.0f, 0.5f, 0.0f);
        for (int i = 0; i < line_renderers.Count; i++)
        {
            line_renderers[i].SetPosition(0, self_ray_source);
            line_renderers[i].SetPosition(1, all_coins[i].transform.position);
            if (selected_push_coin_indicies.Contains(i)) {
                line_renderers[i].colorGradient = selected_push_gradient;
            }
            else if (selected_pull_coin_indicies.Contains(i)) {
                line_renderers[i].colorGradient = selected_pull_gradient;
            } else {
                line_renderers[i].colorGradient = unselected_gradient;
            }
        }

        //push pushed coins
        float push_level = Input.GetKey("v")? 1.0f : 0.0f;
        float pull_level = Input.GetKey("b")? 1.0f : 0.0f;
        ResolveForces(push_level, pull_level);

        //todo:
        //concept of burn level
    }

    void FixedUpdate() {

    }
}
