# Duality Jam

Extend - silver
Fire - red
Life - green
Move - gold
Plague - yellow
Spread - gray
Water - blue
Wild (?) - purple

# Rules

- disaster acts like spread for itself or extend for any other
- can only create with disaster
- can extend/spread either player's controlled disaster (then why does it matter who controls it?)

# Design Questions

- what happens if entire hand is unplayable? (spread/extend with no active disaster)
- extend and especially spread are OP if they can both move and turn any disaster
        - maybe neither changes direction after advancing

## Bugs

- possible Heisenbug: after extending/spreading to a stop point, sometimes asks
  for direction (very rare scenario)
- extend says it's unplayable after disaster ends but can still play it
- setting fire direction may be impossible in some scenarios leading to a softlock

## Features

### Important

- visual for the deadzone (outside tiles)
- don't auto-advance on placement turn
- allow change of disaster if trail >= 3
- quelch fire if touching fire on two sides
- disallow water/fire turning back on itself
- wild card: select which card to copy, use that logic
- resume matching disaster from any disaster tile

### Wishlist

- plague card: select placement (occupied), select facing
- card info from text file
- error checking on initial stones
- remove duplicates from initial stones

- CARD COUNT MUST BE 2n+3
