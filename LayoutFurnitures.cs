using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;

public class LayoutFurnitures : MonoBehaviour
{
    [SerializeField]
    GameObject[] furniturePrefabs;

    [SerializeField]
    GameObject layoutListPanel;

    [SerializeField]
    GameObject buttonAndTogglePrefab;

    [SerializeField]
    Button crossoverButton;

    [SerializeField]
    Button mutationButton;

    [SerializeField]
    GameObject furnitureGroup;

    static int PatternNum = 30;

    // Start is called before the first frame update
    void Start()
    {
        var allFurniturePosList = new Dictionary<string, Dictionary<string, Vector3>>();

        // Generate
        for (int i = 0; i < PatternNum; i++)
        {
            var name = i.ToString("000");

            var furniturePosList = new Dictionary<string, Vector3>();
            foreach (var furniturePrefab in furniturePrefabs)
            {
                // Layout randomly
                var pos = new Vector3(Random.Range(-5, 10), 0, Random.Range(-5, 5));
                furniturePosList.Add(furniturePrefab.name, pos);
            }
            allFurniturePosList[name] = furniturePosList;
 
            var buttonAndToggle = Instantiate(buttonAndTogglePrefab);
            buttonAndToggle.name = name;
            buttonAndToggle.GetComponentInChildren<TextMeshProUGUI>().text = name;
            buttonAndToggle.transform.SetParent(layoutListPanel.transform);
            var button = buttonAndToggle.GetComponentInChildren<Button>();
            button.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    // Clear all furniture
                    furnitureGroup.GetComponentsInChildren<Transform>().Where(o => o.name != "FurnitureGroup").ToList().ForEach(o => Destroy(o.gameObject));

                    foreach (var furniturePrefab in furniturePrefabs)
                    {
                        var furniture = Instantiate(furniturePrefab);
                        furniture.transform.parent = furnitureGroup.transform;

                        var pos = furniturePosList[furniturePrefab.name];
                        furniture.transform.localPosition = new Vector3(pos.x, furniture.transform.position.y, pos.z);
                        furniture.transform.rotation = furniture.transform.rotation;
                    }
                });
        }

        // Crossover
        crossoverButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Crossover");

                // Print positions toggled objects.
                var toggles = layoutListPanel.GetComponentsInChildren<Toggle>().Where(o => o.isOn).Select(o => o.gameObject).ToList();
                foreach (var toggle in toggles)
                {
                    var list = allFurniturePosList[toggle.transform.parent.parent.name];
                    foreach (var obj in list)
                    {
                        Debug.Log(obj.Key + ":" + obj.Value.ToString());


                    }
                }

                foreach (var toggle in toggles)
                {
                    var 





                }

            });

        // Mutation
        mutationButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Mutation");

                // Write great code.

            });
    }
}
