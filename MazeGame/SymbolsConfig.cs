﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame
{
    /// <summary>
    /// A helper class that holds in a centralized place all the gameplay relevant symbols
    /// </summary>
    public static class SymbolsConfig
    {
        public const char ExitChar = 'Ð';
        public const char EntranceChar = '≡';
        public const char KeyChar = '¶';
        public const char SpawnChar = 'X';
        public const char EmptySpace = ' ';
        public const char TreasureChar = '$';
        public const char LeverOffChar = '\\';
        public const char LeverOnChar = '/';
        public const char GateChar = '#';


        public const string OutroArt = @"
  
                                         o
                                    .-'''|
                                    |,-''|
                                         |   _.- '`.
                                        _|-''_.-'|. `.
                                       |:^.-'_.-'`.;. `.
                                       | `.'.   ,-'_.-'|
                                       |   + '-'.-'   J
                    __.            .d88|    `.-'      |
               _.--'_..`.    .d88888888|     |       J'b.
            +:' ,--'_.|`.`.d88888888888|-.   |    _-.|888b.
            | \ \-'_.--'_.-+888888888+'  _>F F +:'   `88888bo.
             L \ +'_.--'   |88888+''  _.' J J J  `.    +8888888b.
             |  `+'        |8+''  _.-'    | | |    +    `+8888888._-'.
           .d8L L         J _.-'          | | |     `.    `+888+^'.-|.`.
          d888|  |         J-'            F F F       `.  _.-'_.-'_.+.`.`.
         d88888L L     _.   L            J J J          `|. +'_.-'    `_+ `;
         888888J  |  +-'  \ L         _.-+.|.+.          F `.`.     .-'_.-'J
         8888888|  L L\    \|     _.-'     '   `.       J    `.`.,-'.-'    |
         8888888PL | | \    `._.-'               `.     |      `..-'      J.b
         8888888 |  L L `.    \     _.-+.          `.   L+`.     |        F88b
         8888888  L | |   \   _..--'_.-|.`.          >-'    `., J        |8888b
         8888888  |  L L   +:' _.--'_.-'.`.`.    _.-'     .-' | |       JY88888b
         8888888   L | |   J \ \_.-'     `.`.`.-'     _.-'   J J        F Y88888b
         Y888888    \ L L   L \ `.      _.-'_.-+  _.-'       | |       |   Y88888b
         `888888b    \| |   |  `. \ _.-'_.-'   |-'          J J       J     Y88888b
          Y888888     +'\   J    \ '_.-'       F    ,-T'\   | |    .-'      )888888
           Y88888b.      \   L    +'          J    /  | J  J J  .-'        .d888888
            Y888888b      \  |    |           |    F  '.|.-'+|-'         .d88888888
             Y888888b      \ J    |           F   J    -.              .od88888888P
              Y888888b      \ L   |          J    | .' ` \d8888888888888888888888P
               Y888888b      \|   |          |  .-'`.  `\ `.88888888888888888888P
                Y888888b.     J   |          F-'     \\ ` \ \88888888888888888P'
                 Y8888888b     L  |         J     d8`.`\  \`.8888888888888P'
                  Y8888888b    |  |        .+      d8888\  ` .'  `Y888888P'
                  `88888888b   J  |     .-'     .od888888\.-'
                   Y88888888b   \ |  .-'     d888888888P'
                   `888888888b   \|-'       d888888888P       ,0\
                    `Y88888888b            d8888888P'        ,\'^6
                      Y88888888bo.      .od88888888 hs         988b
                      `8888888888888888888888888888              §8
                       Y88888888888888888888888888P                °
                       `Y8888888888888888888888P'
                         `Y8888888888888P'
                              `Y88888P'";
    }
}
