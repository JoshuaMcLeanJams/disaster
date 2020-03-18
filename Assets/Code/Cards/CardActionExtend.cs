﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionAdvance: CardAction
{

    public override bool IsPlayable => Board.instance.HasControlledDisaster;

    private BoardTile m_target = null;
    private int m_moveSpeed = 1;

    public CardActionAdvance() : this(null) { }
    public CardActionAdvance(Player a_owner, int a_moveSpeed=1) : base(a_owner) {
        m_moveSpeed = a_moveSpeed;
    }

    public override void Activate() {
        if (m_target == null) {
            Board.instance.ToggleTiles((t) => {
                return t.HasControlledDisaster;
            });
            return;
        }
        Board.instance.ToggleTiles((t) => {
            return t.IsAdjacentOrthogonalTo(m_target, m_moveSpeed);
        });
    }

    public override bool Execute(BoardTile a_tile) {
        if (m_target == null) {
            m_target = a_tile;
            return false;
        }
        for (var i = 0; i < m_moveSpeed; ++i) {
            m_target.Disaster.Advance();
        }
        return true;
    }
}

public class CardActionExtend : CardActionAdvance
{
    public CardActionExtend() : this(null) { }
    public CardActionExtend(Player a_owner) : base(a_owner, 2) {
        Color = new Color(0.6f, 0.6f, 0.6f);
        Info = "Disaster ~ Extend (1)";
    }
}
    
public class CardActionSpread : CardActionAdvance
{
    public CardActionSpread() : this(null) { }
    public CardActionSpread(Player a_owner) : base(a_owner, 2) {
        Color = new Color(0.6f, 0.6f, 0.6f);
        Info = "Disaster ~ Spread (2)";
    }
}
