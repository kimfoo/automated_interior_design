using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

public class LayoutFurnitures : MonoBehaviour
{
    [SerializeField]
    GameObject[] furniturePrefabs;

    [SerializeField]
    GameObject[] walls;

    [SerializeField]
    GameObject layoutListPanel;

    [SerializeField]
    GameObject buttonAndTogglePrefab;

    [SerializeField]
    GameObject house;

    [SerializeField]
    Button crossoverButton;

    [SerializeField]
    Button mutationButton;

    [SerializeField]
    GameObject furnitureGroup;

    [SerializeField]
    GameObject layoutListPanelForCrossover;



    static int PatternNum = 30;

    // Start is called before the first frame update
    void Start()
    {
        var allFurniturePosList = new Dictionary<string, Dictionary<string, Vector3>>();
        var allFurniturePosListForCrossover = new Dictionary<string, Dictionary<string, Vector3>>();
        var allFurniturePosListForMutation = new Dictionary<string, Dictionary<string, Vector3>>();

        // Generate
        for (int i = 0; i < PatternNum; i++)
        {
            var name = i.ToString("000");

            var furniturePosList = new Dictionary<string, Vector3>();

            int num = Random.Range(3, furniturePrefabs.Length);
            var numList = new List<int>();
            for (int j = 0; j < furniturePrefabs.Length; j++) numList.Add(j);
            for (int z = 0; z < num; z++)
            {
                int value = numList[Random.Range(0, numList.Count)];
                numList.Remove(value);
                var furniturePrefab = furniturePrefabs[value];

                var count = 100;
                while (count > 0)
                {
                    bool intersect = false;

                    var furniture1 = Instantiate(furniturePrefab);
                    furniture1.name = furniturePrefab.name;
                    furniture1.transform.parent = furnitureGroup.transform;
                    furniture1.AddComponent<BoxCollider>();
                    var pos = new Vector3(Random.Range(0, (9.17f - 0.37f)) + 0.37f, furniturePrefab.transform.position.y, -Random.Range(0, (7.59f - 0.74f) + 0.74f));

                    foreach (var wall in walls)
                    {
                        var boxCollider1 = furniture1.GetComponent<BoxCollider>();
                        var boxCollider2 = wall.GetComponent<BoxCollider>();
                        var bounds1 = new Bounds(pos + house.transform.position, boxCollider1.bounds.size);
                        var bounds2 = new Bounds(wall.transform.position, boxCollider2.bounds.size);
                        if (bounds1.Intersects(bounds2))
                        {
                            intersect = true;
                            break;
                        }
                    }

                    if (intersect == true)
                    {
                        Destroy(furniture1);
                        continue;
                    }

                    if (furniturePosList.Count == 0)
                    {

                    }
                    else
                    {
                        foreach (var furniture2Dict in furniturePosList)
                        {
                            if (furniture1.name == furniture2Dict.Key) continue;

                            var boxCollider1 = furniture1.GetComponent<BoxCollider>();
                            var furniture2 = GameObject.Find(furniture2Dict.Key);
                            var boxCollider2 = furniture2.GetComponent<BoxCollider>();
                            var bounds1 = new Bounds(pos + house.transform.position, boxCollider1.bounds.size);
                            var bounds2 = new Bounds(furniturePosList[furniture2.name] + house.transform.position, boxCollider2.bounds.size);
                            if (bounds1.Intersects(bounds2))
                            {
                                intersect = true;
                                break;
                            }
                        }
                    }

                    Destroy(furniture1);

                    if (intersect == false)
                    {
                        furniturePosList.Add(furniturePrefab.name, pos);
                        break;
                    }

                    count--;
                }

                furnitureGroup.GetComponentsInChildren<Transform>().Where(o => o.name != "FurnitureGroup").ToList().ForEach(o => Destroy(o.gameObject));
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
                        if (furniturePosList.ContainsKey(furniturePrefab.name))
                        {
                            var furniture = Instantiate(furniturePrefab);
                            furniture.transform.parent = furnitureGroup.transform;

                            var pos = furniturePosList[furniturePrefab.name];
                            furniture.transform.localPosition = new Vector3(pos.x, furniture.transform.position.y, pos.z);
                            furniture.transform.rotation = furniture.transform.rotation;
                        }                        
                    }
                });
        }

        // Crossover
        crossoverButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Crossover");
                allFurniturePosListForCrossover.Clear();
                layoutListPanelForCrossover.GetComponentsInChildren<Transform>().Where(o => o.name != "Content").ToList().ForEach(o => Destroy(o.gameObject));

                // Print positions toggled objects.

                var toggles = layoutListPanel.GetComponentsInChildren<Toggle>().Where(o => o.isOn).Select(o => o.gameObject).ToList();

                //int rnd = Random.Range(5, furniturePrefabs.Length);

                var index_crossover = 0;
                for (int i = 0; i < toggles.Count - 1; i++)
                {
                    for (int j = i + 1 ; j < toggles.Count; j++)
                    {
                        var toggle1 = toggles[i];
                        var toggle2 = toggles[j];

                        var furniturePosListForCrossover = new Dictionary<string, Vector3>();

                        var list1 = allFurniturePosList[toggle1.transform.parent.parent.name];
                        var list2 = allFurniturePosList[toggle2.transform.parent.parent.name];

                        foreach (var furniture1 in list1.Keys)
                        {
                            furniturePosListForCrossover.Add(furniture1, list1[furniture1]);

                            foreach (var furniture2 in list2.Keys)
                            {
                                //Debug.Log(furniture1 + ":" + furniture2);
                            

                                // Compare two funiture here

                                if (!list1.Keys.Contains(furniture2))
                                {
                                    // Crossover two furniture

                                    // Add new furniture to the listForCrossOver

                                    if (furniturePosListForCrossover.ContainsKey(furniture2) == false)
                                    {
                                        furniturePosListForCrossover.Add(furniture2, list2[furniture2]);
                                        
                                    }

                                }
                            }
                        }

                        allFurniturePosListForCrossover.Add(index_crossover.ToString("000"), furniturePosListForCrossover);
                        index_crossover++;
                    }
                }

                foreach (var key in allFurniturePosListForCrossover.Keys)
                {
                    var buttonAndToggle = Instantiate(buttonAndTogglePrefab);
                    buttonAndToggle.name = key;
                    buttonAndToggle.GetComponentInChildren<TextMeshProUGUI>().text = key;
                    buttonAndToggle.transform.SetParent(layoutListPanelForCrossover.transform);
                    var button = buttonAndToggle.GetComponentInChildren<Button>();
                    button.OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            // Clear all furniture
                            furnitureGroup.GetComponentsInChildren<Transform>().Where(o => o.name != "FurnitureGroup").ToList().ForEach(o => Destroy(o.gameObject));

                            var furniturePosListForCrossover = allFurniturePosListForCrossover[key];

                            foreach (var furniturePrefab in furniturePrefabs)
                            {
                                if (furniturePosListForCrossover.ContainsKey(furniturePrefab.name))
                                {
                                    var furniture = Instantiate(furniturePrefab);
                                    furniture.transform.parent = furnitureGroup.transform;

                                    var pos = furniturePosListForCrossover[furniturePrefab.name];
                                    furniture.transform.localPosition = new Vector3(pos.x, furniture.transform.position.y, pos.z);
                                    furniture.transform.rotation = furniture.transform.rotation;
                                }
                            }
                        });
                }
            });

         // Mutation
         mutationButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Mutation");

                allFurniturePosListForMutation.Clear();
                //layoutListPanelForCrossover.GetComponentsInChildren<Transform>().Where(o => o.name != "Content").ToList().ForEach(o => Destroy(o.gameObject));


                var toggles = layoutListPanelForCrossover.GetComponentsInChildren<Toggle>().Where(o => o.isOn).Select(o => o.gameObject).ToList();
                var furniturePosListForMutation = new Dictionary<string, Vector3>();
                var _furniturePosListForMutation = new Dictionary<string, Vector3>();

                var count = 0;
                foreach (var toggle in toggles)
                {
                    var list = allFurniturePosListForCrossover[toggle.transform.parent.parent.name];

                    int num = Random.Range(1, 3);
                    var numList = new List<int>();
                    for (int j = 0; j < furniturePrefabs.Length; j++) numList.Add(j);
                    for (int z = 0; z < num; z++)
                    {
                        int value = numList[Random.Range(0, numList.Count)];
                        numList.Remove(value);
                        var furniturePrefab = furniturePrefabs[value];

                        if (!furniturePosListForMutation.ContainsKey(furniturePrefab.name))
                        {
                            furniturePosListForMutation.Add(furniturePrefab.name, list[furniturePrefab.name]);  // sometimes an error occurs
                            foreach (var listkey in furniturePosListForMutation)
                            {
                                Debug.Log("object to be mutated:"+ listkey);

                            }
                        }

                    }
                    _furniturePosListForMutation = furniturePosListForMutation.ToDictionary(entry => entry.Key, entry => entry.Value);

                

                    //foreach (var furniturekey in list.Keys)
                    //{
                    //    Debug.Log("furniturekey:" + furniturekey);

                        //foreach (var for_mutation in _furniturePosListForMutation)

                        //{
                           
                        //    foreach (var furniturePrefab in furniturePrefabs)
                        //    {
                        //        var mutated = Instantiate(furniturePrefab);
                        //        mutated.name = furniturePrefab.name;
                        //        mutated.transform.parent = furnitureGroup.transform;

                        //        //new position and orientation
                        //        var pos = new Vector3(Random.Range(0, (9.17f - 0.37f)) + 0.37f, furniturePrefab.transform.position.y, -Random.Range(0, (7.59f - 0.74f) + 0.74f));
                        //        mutated.transform.localPosition = new Vector3(pos.x, furniturePrefab.transform.position.y, pos.z);
                        //        mutated.transform.rotation = Quaternion.Euler(0, 90f, 0);

                        //        Debug.Log("mutated:" + mutated.name);

                        //    }


                        //}

                        //Destroy(GameObject.Find(furniturekey));

                        //if (!furniturePosListForMutation.ContainsKey(furniturekey))
                        //{
                        //    _furniturePosListForMutation.Add(furniturekey, list[furniturekey]);
                        //}
                    //}

                }   

 
                allFurniturePosListForMutation[count.ToString("000")] = furniturePosListForMutation;
                count++;
                

                // clear previous buttons
                layoutListPanel.GetComponentsInChildren<Button>().ToList().ForEach(o => Destroy(o.gameObject.transform.parent.gameObject));

                foreach (var key in allFurniturePosListForMutation.Keys)
                {
                    var buttonAndToggle = Instantiate(buttonAndTogglePrefab);
                    buttonAndToggle.name = key;
                    buttonAndToggle.GetComponentInChildren<TextMeshProUGUI>().text = key;
                    buttonAndToggle.transform.SetParent(layoutListPanel.transform);
                    var button = buttonAndToggle.GetComponentInChildren<Button>();
                    button.OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            

                            // Clear all furniture
                            furnitureGroup.GetComponentsInChildren<Transform>().Where(o => o.name != "FurnitureGroup").ToList().ForEach(o => Destroy(o.gameObject));

                            var furniturePosListForMutation = allFurniturePosListForMutation[key];


                            foreach (var furniturePrefab in furniturePrefabs)
                            {
                                if (furniturePosListForMutation.ContainsKey(furniturePrefab.name))
                                {
                                    var furniture = Instantiate(furniturePrefab);
                                    furniture.transform.parent = furnitureGroup.transform;

                                    var pos = furniturePosListForMutation[furniturePrefab.name];
                                    furniture.transform.localPosition = new Vector3(pos.x, furniture.transform.position.y, pos.z);
                                    furniture.transform.rotation = furniture.transform.rotation;
                                }

                            }
                        });
                }



            });
        }
}

