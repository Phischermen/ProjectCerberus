using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    [CustomEditor(typeof(PuzzleContainer))]
    public class PuzzleContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Translate Right"))
            {
                var puzzleContainer = (PuzzleContainer) target;
                var tilemap = puzzleContainer.GetComponentInChildren<Tilemap>();
                var bounds = tilemap.cellBounds;
                // Iterate across tiles backwards
                for (int i = bounds.yMax - 1; i >= bounds.yMin; i--)
                {
                    for (int j = bounds.xMax - 1; j >= bounds.xMin; j--)
                    {
                        // Get current tile
                        var tile = tilemap.GetTile(new Vector3Int(j, i, 0));

                        // Set right adjacent tile to cached tile. Erase original.
                        tilemap.SetTile(new Vector3Int(j + 1, i, 0), tile);
                        tilemap.SetTile(new Vector3Int(j, i, 0), null);
                    }
                }

                // Translate children
                for (var i = 0; i < puzzleContainer.transform.childCount; i++)
                {
                    var child = puzzleContainer.transform.GetChild(i);
                    //Skip tilemap
                    if (child == tilemap.transform) continue;
                    child.Translate(Vector3.right * tilemap.layoutGrid.cellSize.x);
                }
            }

            if (GUILayout.Button("Translate Left"))
            {
                var puzzleContainer = (PuzzleContainer) target;
                var tilemap = puzzleContainer.GetComponentInChildren<Tilemap>();
                var bounds = tilemap.cellBounds;
                // Iterate across tiles forwards
                for (int i = bounds.yMin; i < bounds.yMax; i++)
                {
                    for (int j = bounds.xMin; j < bounds.xMax; j++)
                    {
                        // Get current tile
                        var tile = tilemap.GetTile(new Vector3Int(j, i, 0));

                        // Set left adjacent tile to cached tile. Erase original.
                        tilemap.SetTile(new Vector3Int(j - 1, i, 0), tile);
                        tilemap.SetTile(new Vector3Int(j, i, 0), null);
                    }
                }

                // Translate children
                for (var i = 0; i < puzzleContainer.transform.childCount; i++)
                {
                    var child = puzzleContainer.transform.GetChild(i);
                    //Skip tilemap
                    if (child == tilemap.transform) continue;
                    child.Translate(Vector3.left * tilemap.layoutGrid.cellSize.x);
                }
            }

            if (GUILayout.Button("Translate Up"))
            {
                var puzzleContainer = (PuzzleContainer) target;
                var tilemap = puzzleContainer.GetComponentInChildren<Tilemap>();
                tilemap.CompressBounds();
                var bounds = tilemap.cellBounds;
                // Iterate across tiles backwards
                for (int i = bounds.yMax - 1; i >= bounds.yMin; i--)
                {
                    for (int j = bounds.xMax - 1; j >= bounds.xMin; j--)
                    {
                        // Get current tile
                        var tile = tilemap.GetTile(new Vector3Int(j, i, 0));

                        // Set right adjacent tile to cached tile. Erase original.
                        tilemap.SetTile(new Vector3Int(j, i + 1, 0), tile);
                        tilemap.SetTile(new Vector3Int(j, i, 0), null);
                    }
                }

                // Translate children
                for (var i = 0; i < puzzleContainer.transform.childCount; i++)
                {
                    var child = puzzleContainer.transform.GetChild(i);
                    //Skip tilemap
                    if (child == tilemap.transform) continue;
                    child.Translate(Vector3.up * tilemap.layoutGrid.cellSize.y);
                }
            }

            if (GUILayout.Button("Translate Down"))
            {
                var puzzleContainer = (PuzzleContainer) target;
                var tilemap = puzzleContainer.GetComponentInChildren<Tilemap>();
                var bounds = tilemap.cellBounds;
                // Iterate across tiles forwards
                for (int i = bounds.yMin; i < bounds.yMax; i++)
                {
                    for (int j = bounds.xMin; j < bounds.xMax; j++)
                    {
                        // Get current tile
                        var tile = tilemap.GetTile(new Vector3Int(j, i, 0));

                        // Set left adjacent tile to cached tile. Erase original.
                        tilemap.SetTile(new Vector3Int(j, i - 1, 0), tile);
                        tilemap.SetTile(new Vector3Int(j, i, 0), null);
                    }
                }

                // Translate children
                for (var i = 0; i < puzzleContainer.transform.childCount; i++)
                {
                    var child = puzzleContainer.transform.GetChild(i);
                    //Skip tilemap
                    if (child == tilemap.transform) continue;
                    child.Translate(Vector3.down * tilemap.layoutGrid.cellSize.y);
                }
            }

            if (GUILayout.Button("Rotate 90°"))
            {
                var puzzleContainer = (PuzzleContainer) target;
                var tilemap = puzzleContainer.GetComponentInChildren<Tilemap>();
                tilemap.CompressBounds();
                var bounds = tilemap.cellBounds;
                // Iterate across tiles forwards
                for (int i = bounds.yMin; i < bounds.yMax; i++)
                {
                    for (int j = bounds.xMin; j < bounds.xMax; j++)
                    {
                        // Get current tile
                        var tile = tilemap.GetTile(new Vector3Int(j, i, 0));

                        // Set tile in next quadrant to cached tile. Erase original.
                        tilemap.SetTile(new Vector3Int(-i, j, 0), tile);
                        //Debug.Log($"Processing point ({i},{j})");
                        tilemap.SetTile(new Vector3Int(j, i, 0), null);
                    }
                }

                // Translate children
                for (var i = 0; i < puzzleContainer.transform.childCount; i++)
                {
                    var child = puzzleContainer.transform.GetChild(i);
                    //Skip tilemap
                    if (child == tilemap.transform) continue;
                    var transform = child.transform;
                    var position = transform.position + new Vector3(0, -1, 0);
                    position = new Vector3(-position.y, position.x, position.z);
                    transform.position = position;
                }
            }
        }
    }
}