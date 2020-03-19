﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    public static Board instance = null;

    [SerializeField] private Player m_playerBlack = null;
    [SerializeField] private Player m_playerWhite = null;
    [SerializeField] private TextMeshProUGUI m_infoTextMesh = null;

    [Header("Game Design")]
    [SerializeField] private int m_boardSizeTiles = 6;
    [SerializeField] private int m_boardSizePixels = 600;
    [SerializeField] private int m_handSize = 5;
    [SerializeField] private int m_playPerTurn = 2;
    [SerializeField] private bool m_autoAdvance = false;
    [SerializeField] private bool m_extendAlsoTurns = false;
    [SerializeField] private bool m_spreadAlsoTurns = false;

    [Header("Board Setup")]
    [SerializeField] private List<Vector2Int> m_initialStonesBlack = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> m_initialStonesWhite = new List<Vector2Int>();
    
    [Header("Sprites")]
    [SerializeField] private Sprite m_spriteStoneBlack = null;
    [SerializeField] private Sprite m_spriteStoneWhite = null;
    [SerializeField] private Sprite m_spriteDisasterFire = null;
    [SerializeField] private Sprite m_spriteDisasterPlague = null;
    [SerializeField] private Sprite m_spriteDisasterWater = null;
    [SerializeField] private Sprite m_spriteDisasterFireBlack = null;
    [SerializeField] private Sprite m_spriteDisasterFireBlackNeutral = null;
    [SerializeField] private Sprite m_spriteDisasterPlagueBlack = null;
    [SerializeField] private Sprite m_spriteDisasterPlagueBlackNeutral = null;
    [SerializeField] private Sprite m_spriteDisasterWaterBlack = null;
    [SerializeField] private Sprite m_spriteDisasterWaterBlackNeutral = null;
    [SerializeField] private Sprite m_spriteDisasterFireWhite = null;
    [SerializeField] private Sprite m_spriteDisasterFireWhiteNeutral = null;
    [SerializeField] private Sprite m_spriteDisasterPlagueWhite = null;
    [SerializeField] private Sprite m_spriteDisasterPlagueWhiteNeutral = null;
    [SerializeField] private Sprite m_spriteDisasterWaterWhite = null;
    [SerializeField] private Sprite m_spriteDisasterWaterWhiteNeutral = null;

    [Header("Prefabs")]
    [SerializeField] private Card m_cardPrefab = null;
    [SerializeField] private BoardTile m_tilePrefab = null;

    public bool ExtendAlsoTurns => m_extendAlsoTurns;
    public bool SpreadAlsoTurns => m_spreadAlsoTurns;

    public float TileSize => (float)m_boardSizePixels / m_boardSizeTiles;
    public Card CardPrefab => m_cardPrefab;
    public int HandSize => m_handSize;
    public Player ActivePlayer { get; private set; } = null;
    public Player PlayerBlack => m_playerBlack;
    public Player PlayerWhite => m_playerWhite;
    public int PlayPerTurn => m_playPerTurn;

    public bool HasControlledDisaster => PlayerBlack.HasControlledDisaster || PlayerWhite.HasControlledDisaster;
    public bool HasClearSpace {
        get {
            foreach (var tile in m_tileMap)
                if (tile.IsClear)
                    return true;
            return false;
        }
    }

    public string InfoText {
        set {
            if (m_isGameOver)
                return;
            m_infoTextMesh.text = value;
        }
    }

    private Card m_activeCard = null;

    private BoardTile[,] m_tileMap = null;
    private int m_tileSize = 0;
    public int ActiveTileCount { get; private set; } = 0;

    public Card ActiveCard {
        private get => m_activeCard;
        set {
            if( m_activeCard != null) {
                m_activeCard.IsCardActive = false;
                if (m_activeCard == value) {
                    m_activeCard = null;
                    return;
                }
            }
            m_activeCard = value;
            if (m_activeCard == null)
                return;
            m_activeCard.IsCardActive = true;
        }
    }

    public void ActivateCard(BoardTile a_tile) {
        if (ActiveCard == null)
            return;
        Debug.Log($"Play [{ActiveCard.CardType}] on ({a_tile.x}, {a_tile.y})");
        ActiveCard.Play(a_tile);
    }

    public List<BoardTile> GetNeighborsDiagonal(int a_x, int a_y, int a_distance = 1) {
        var tileList = new List<BoardTile>();
        var stepList = new List<int>();
        for (var i = 1; i <= a_distance; ++i) {
            stepList.Add(i);
            stepList.Add(-i);
        }
        foreach (var oy in stepList) {
            foreach (var ox in stepList) {
                var tile = GetTile(a_x + ox, a_y + oy);
                if (tile == null)
                    continue;
                tileList.Add(tile);
            }
        }
        return tileList;
    }

    public List<BoardTile> GetNeighborsOrthogonal(int a_x, int a_y, int a_distance = 1) {
        var tileList = new List<BoardTile>();
        var stepList = new List<int>();
        for (var i = 1; i <= a_distance; ++i) {
            stepList.Add(i);
            stepList.Add(-i);
        }
        foreach (var ox in stepList) {
            var tile = GetTile(a_x + ox, a_y);
            if (tile == null)
                continue;
            tileList.Add(tile);
        }
        foreach (var oy in stepList) {
            var tile = GetTile(a_x, a_y + oy);
            if (tile == null)
                continue;
            tileList.Add(tile);
        }
        return tileList;
    }

    public bool HasNeighborBoth(int a_x, int a_y, PlayerColor a_color, int a_distance=1) {
        return HasNeighborOrthogonal(a_x, a_y, a_color, a_distance)
            || HasNeighborDiagonal(a_x, a_y, a_color, a_distance);
    }

    public bool HasNeighborDiagonal(int a_x, int a_y, PlayerColor a_color, int a_distance = 1) {
        var list = GetNeighborsDiagonal(a_x, a_y, a_distance);
        if (list == null)
            return false;
        foreach (var tile in list) {
            if (TileMatch(tile.x, tile.y, a_color))
                return true;
        }
        return false;
    }

    public bool HasNeighborOrthogonal(int a_x, int a_y, PlayerColor a_color, int a_distance = 1) {
        var list = GetNeighborsOrthogonal(a_x, a_y, a_distance);
        if (list == null)
            return false;
        foreach (var tile in list) {
            if (TileMatch(tile.x, tile.y, a_color))
                return true;
        }
        return false;
    }

    public void EnableInput() {
        ActivePlayer.CardsEnabled = true;
    }

    public void DisableInput() {
        PlayerBlack.CardsEnabled = false;
        PlayerWhite.CardsEnabled = false;
    }

    public void EndGame() {
        var winner = "Tie";
        if (m_playerBlack.Score > m_playerWhite.Score)
            winner = "Black";
        else if (m_playerWhite.Score > m_playerBlack.Score)
            winner = "White";
        InfoText = $"Game over! Winner: {winner}";
        m_isGameOver = true;
    }

    private bool m_isGameOver = false;

    public void ToggleTiles(System.Func<BoardTile, bool> a_isInteractable) {
        ActiveTileCount = 0;
        for (var y = 0; y < m_boardSizeTiles; ++y) {
            for (var x = 0; x < m_boardSizeTiles; ++x) {
                var button = m_tileMap[x, y].GetComponent<Button>();
                button.interactable = a_isInteractable(m_tileMap[x, y]);
                if (button.interactable)
                    ++ActiveTileCount;
            }
        }
    }

    public void ResetTiles() {
        ToggleTiles((t) => {
            return true;
        });
    }

    public Sprite GetDisasterSprite(DisasterType a_disaster) {
        return a_disaster == DisasterType.Fire
            ? m_spriteDisasterFire
            : a_disaster == DisasterType.Plague
                ? m_spriteDisasterPlague
                : m_spriteDisasterWater;
    }

    public Sprite GetDisasterSprite(DisasterType a_disaster, PlayerColor a_color, bool a_isNeutral) {
        return a_isNeutral
            ? a_color == PlayerColor.Black
                ? a_disaster == DisasterType.Fire
                    ? m_spriteDisasterFireBlackNeutral
                    : a_disaster == DisasterType.Plague
                        ? m_spriteDisasterPlagueBlackNeutral
                        : m_spriteDisasterWaterBlackNeutral
                : a_disaster == DisasterType.Fire
                    ? m_spriteDisasterFireWhiteNeutral
                    : a_disaster == DisasterType.Plague
                        ? m_spriteDisasterPlagueWhiteNeutral
                        : m_spriteDisasterWaterWhiteNeutral
            : a_color == PlayerColor.Black
                ? a_disaster == DisasterType.Fire
                    ? m_spriteDisasterFireBlack
                    : a_disaster == DisasterType.Plague
                        ? m_spriteDisasterPlagueBlack
                        : m_spriteDisasterWaterBlack
                : a_disaster == DisasterType.Fire
                    ? m_spriteDisasterFireWhite
                    : a_disaster == DisasterType.Plague
                        ? m_spriteDisasterPlagueWhite
                        : m_spriteDisasterWaterWhite;
    }

    public Sprite GetStoneSprite(PlayerColor a_color) {
        return a_color == PlayerColor.None 
            ? null 
            : a_color == PlayerColor.Black 
                ? m_spriteStoneBlack : m_spriteStoneWhite;
    }

    public BoardTile GetTile(int a_tileX, int a_tileY) {
        return (a_tileX < 0 || a_tileY < 0 || a_tileX >= m_boardSizeTiles || a_tileY >= m_boardSizeTiles) 
            ? null : m_tileMap[a_tileX, a_tileY];
    }

    public void NextTurn() {
        if (m_autoAdvance && ActivePlayer.ControlledDisaster != null)
            ActivePlayer.ControlledDisaster.Advance();
        ActivePlayer = (ActivePlayer == m_playerBlack) ? m_playerWhite : m_playerBlack;
        Debug.Log($"{ActivePlayer}'s turn");
        UpdateScore();
        if(m_isGameOver == false)
            ActivePlayer.StartTurn();
    }

    public void UpdateScore() {
        var blackScore = 0;
        var whiteScore = 0;
        foreach (var tile in m_tileMap) {
            if (tile.StoneColor == PlayerColor.Black)
                ++blackScore;
            else if (tile.StoneColor == PlayerColor.White)
                ++whiteScore;
        }

        m_playerBlack.Score = blackScore;
        m_playerWhite.Score = whiteScore;

        if (blackScore == 0 || whiteScore == 0 || HasClearSpace == false)
            EndGame();
    }


    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;

        var rect = m_tilePrefab.GetComponent<RectTransform>().rect;
        m_tileSize = Mathf.FloorToInt(rect.width);
    }

    private void Start() {
        m_tileMap = new BoardTile[m_boardSizeTiles, m_boardSizeTiles];
        CreateTileButtons();
        PlaceInitialStones();
        UpdateScore();

        ActivePlayer = m_playerBlack;
        ActivePlayer.StartTurn();
    }

    private bool TileMatch(int a_x, int a_y, PlayerColor a_color) {
        return a_x >= 0 && a_y >= 0 && a_x < m_boardSizeTiles && a_y < m_boardSizeTiles && m_tileMap[a_x, a_y].StoneColor == a_color;
    }

    private void CreateTileButtons() {
        var pos = Vector2Int.zero;
        for (var y = 0; y < m_boardSizeTiles; ++y) {
            for (var x = 0; x < m_boardSizeTiles; ++x) {
                m_tileMap[x, y] = Instantiate(m_tilePrefab, transform);
                m_tileMap[x, y].name = $"Board Tile {x} {y}";
                m_tileMap[x, y].x = x;
                m_tileMap[x, y].y = y;
                var rectTransform = m_tileMap[x, y].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = pos;
                pos.x += Mathf.FloorToInt(TileSize);
            }
            pos.x = 0;
            pos.y -= Mathf.FloorToInt(TileSize);
        }
    }

    private void PlaceInitialStones() {
        foreach (var coord in m_initialStonesBlack)
            m_tileMap[coord.x, coord.y].Controller = m_playerBlack;
        foreach (var coord in m_initialStonesWhite)
            m_tileMap[coord.x, coord.y].Controller = m_playerWhite;
    }

}
