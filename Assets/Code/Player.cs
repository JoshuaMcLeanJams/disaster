﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor
{
    Black = 0x00,
    White = 0x01
}

public class Player : MonoBehaviour
{
    [SerializeField] private bool m_isHuman = true;
    [SerializeField] private PlayerColor m_color = PlayerColor.Black;

    public PlayerColor Color => m_color;
    private Deck m_deck = new Deck();
    private Card[] m_handVisual = null;

    private int m_cardsPlayed = 0;

    public void PlayedCard() {
        ++m_cardsPlayed;
        if (m_cardsPlayed >= TurnManager.instance.PlayPerTurn)
            TurnManager.instance.NextTurn();
    }

    public void StartTurn() {
        DrawCards();
    }

    private void DrawCards() {
        var count = TurnManager.instance.HandSize - m_cardsPlayed;
        for (var i = 0; i < count; ++i) {
            var cardType = m_deck.Draw();

            if (m_isHuman) {
                for (var k = 0; k < m_handVisual.Length; ++k) {
                    if (m_handVisual[k].CardType == CardType.None) {
                        m_handVisual[k].CardType = cardType;
                        break;
                    }
                }
            }
        }
        m_cardsPlayed = 0;
    }

    private void Awake() {
        for (var i = 0; i < 4; ++i) {
            m_deck.Add(CardType.Move);
            m_deck.Add(CardType.Life);
            m_deck.Add(CardType.Step);
        }
        for (var i = 0; i < 2; ++i) {
            m_deck.Add(CardType.Fire);
            m_deck.Add(CardType.Water);
            m_deck.Add(CardType.Plague);
            m_deck.Add(CardType.Spread);
        }
        m_deck.Add(CardType.Wild);

        m_deck.Shuffle();
        InitializeVisualHand();
    }

    private void InitializeVisualHand() {
        if (m_isHuman == false)
            return;

        m_handVisual = new Card[TurnManager.instance.HandSize];
        var pos = Vector2Int.zero;
        var cardPrefab = TurnManager.instance.CardPrefab;
        var cardWidth = Mathf.FloorToInt(cardPrefab.GetComponent<RectTransform>().rect.width);
        for (var i = 0; i < TurnManager.instance.HandSize; ++i) {
            var card = Instantiate(TurnManager.instance.CardPrefab, transform);
            card.Owner = this;
            var rectTransform = card.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = pos;
            pos.x += cardWidth;
            m_handVisual[i] = card;
        }
    }
}
