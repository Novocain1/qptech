using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qptech.src
{
    class Multiblock
    {
        /*
         A-wireplate-tin-electric-junction
B-machineryblock
C-itemhatch-*

ABA
BBB
ABA

BCB
BBB
BCB

ABA
BBB
ABA

- need dimensions: x,y,z
- need assembly hammer or something
- right click on NW block
- will lookup patterns
- on match need to:
   - remove blocks
   - add multiblock
- when multi block broken need to return items
- need to handle faces - somehow communicate with hoppers?
    - to start could just use a UI?

- or possibly keep all the blocks and have them form together as a multiblock with different funcitons?
         - keep blocks, hide them, and have the model block overwrite them?
        */
    }
}
