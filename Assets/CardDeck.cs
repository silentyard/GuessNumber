using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    List<Transform> cardsTransform;

    [SerializeField]
    private float xInterval = 12;

    [SerializeField]
    private float zInterval = 9;

    [SerializeField]
    private float guessPhaseDeckPosX = -50;

    [SerializeField]
    private float guessPhaseDeckPosZ = 9;

    [SerializeField]
    private float guessPhaseDeckPosYInterval = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        cardsTransform = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
            cardsTransform.Add(transform.GetChild(i));

        RandomCardsPosition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
  
    void RandomCardsPosition()
    {
        //generate random permutation of position indices
        int n = cardsTransform.Count;
        List<int> index = new List<int>();
        for (int i = 0; i < n; i++)
            index.Add(i);

        for(int i = 0; i < index.Count; i++)
        {
            int swapIndex = Random.Range(i, index.Count);
            int temp = index[i];
            index[i] = index[swapIndex];
            index[swapIndex] = temp;
        }

        //use the permutation to position the cards
        Vector3 startPosition = new Vector3(n % 2 == 0 ? -xInterval * (n / 2 - 0.5f) : -xInterval * n / 2, 0, zInterval);
        for(int i = 0; i < index.Count; i++)
        {
            cardsTransform[i].position = startPosition + new Vector3(xInterval * index[i], 0, 0);
        }
    }

    public void CollectUnchosenCards()
    {
        //remove chosen cards
        for(int i = 0; i < cardsTransform.Count; i++)
        {
            if (cardsTransform[i].GetComponent<Card>().owner != null)
            {
                cardsTransform.RemoveAt(i);
                i--;
            }
        }

        //move unchosen cards
        for(int i = 0; i < cardsTransform.Count; i++)
        {
            Vector3 targetPos = new Vector3(guessPhaseDeckPosX, i * guessPhaseDeckPosYInterval, guessPhaseDeckPosZ);
            cardsTransform[i].GetComponent<Card>().moveTo(targetPos, new Vector3(-90, 0, 0));
        }
    }
}
