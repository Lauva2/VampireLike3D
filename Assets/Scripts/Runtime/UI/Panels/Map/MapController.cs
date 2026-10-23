using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MapController : UIController<MapView, MapModel>
    {
        public MapController(UIManager manager, GameManager game, MapModel model, MapView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("MapClose", manager.CloseModal);
            On("MapNorth", () => Move(Vector2Int.up * 4)); On("MapSouth", () => Move(Vector2Int.down * 4));
            On("MapWest", () => Move(Vector2Int.left * 4)); On("MapEast", () => Move(Vector2Int.right * 4)); On("MapCenter", () => { Model.offset = Vector2Int.zero; Tick(); });
        }
        public override void Render()
        {
            Model.offset = Vector2Int.zero; Tick();
        }
        void Move(Vector2Int delta) { Model.offset += delta; Tick(); }
        public override void Tick() { Model.Refresh(game); View.Present(Model); }
    }
}
