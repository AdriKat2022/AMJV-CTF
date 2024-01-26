# Magic Circus

Produit par :
- Basile ROUX
- Adrien SCHROEDEL

## Contexte
Ce projet a été réalisé dans le cadre de notre deuxième année d'école d'ingénieur du numérique, dans le cours d'*Architectures et Moteurs de Jeux Vidéos* à Télécom SudParis.

La mission de notre binôme consistait à produire un jeu de type CTF ou "Capture The Flag" en respectant un GDD devant être basé sur les attentes des encadrants.


| Date de début | Date de fin |
| --- | --- |
| Jeudi 30 Novembre 2023 | Jeudi 25 Janvier 2024 |

> Total de ~**2 mois** de développement

## Description du jeu
Magic Circus est un jeu vidéo de type "Capture The Flag" fait sous Unity.
Dans ce jeu, vous incarnez les unités attaquantes, qui à l'aide d'attaques et de pouvoirs spéciaux, vont tenter de rapporter le drapeau de l'équipe adverse dans votre camp.


### Déroulement d'une partie
Chaque niveau comporte une ou plusieurs zones de départ pour les unités attaquantes, ainsi que les unités elles-mêmes : les attaquantes contrôlées par le joueur, et les défenseuses contrôlées par l'ordinateur sont toutes prédisposées sur la carte du niveau.

Au début de la partie, le joueur est libre d'examiner la carte et établir une stratégie. Pendant cette phase, il n'est pas possible d'intéragir avec les unités. Lorsque prêt, le joueur peut lancer la partie en appuyant sur "Espace" et rendre les unités réceptives aux ordres.

Les unités sont désormais toutes à l'écoute de leur maître associé : une IA basique pour les unités défenseuses et le joueur pour les unités attaquantes.

> Une unité attaquante devient **ROI** lorsqu'elle s'empare du **drappeau**

### Condition de victoire des attaquants
Pour remporter la victoire, il existe deux possibilités :
- L'unité devenue ROI a pu se rendre sur une des ses zones de départ
- L'équipe adverse a été complètement éliminée

### Condition de victoire des défenseurs
En revanche, le joueur perd la partie si l'une des deux conditions suivantes se réalise :
- L'unité devenue ROI a été vaincue
- L'équipe attaquante a été complètement éliminée


## Contrôles du joueur


### Caméra libre et flexible
La caméra est libre et permet au joueur d'adopter l'angle de vu qu'il préfère.
| Touche(s) | Action |
| ---- | ---- |
| **Z Q S D** | Déplacer la caméra sur un plan parallèle à la carte |
| Souris molette | Contrôle du zoom de la caméra |
| Souris molette + ALT GAUCHE | Contrôle de la taille du texte des unités |
| Clic droit | Orienter la caméra vers le point cliqué |

### Contrôle des unités

Le joueur possède de multiples façons pour intéragir avec ses unités. Celles-ci se basent sur le concept de **sélection**.

> **Il n'est pas possible de sélectionner une unité adverse.**

Lorsqu'aucune unité n'est sélectionnée :
| Action | Résultat |
| ---- | ---- |
| Clic gauche sur unité | **Sélectionne cette unité** |
| Clic gauche glissé | **Sélectionne les unités contenue dans le rectangle tracé** |

---

Lorsqu'une ou plusieurs unités sont sélectionnées :
| Action | Ordre donné à ou aux unités sélectionnées |
| ---- | ---- |
| Clic gauche sur terrain | **Déplacement à l'endroit pointé** |
| Clic gauche sur unité | **Chasser et attaquer l'unité pointée** |
| Clic gauche glissé[*](#footnote1) | **Sélectionne les unités contenue dans le rectangle tracé** |
| Touche X | **Déselectionne toutes les unités** |
| Touche A | **Active l'attaque spéciale si elle est prête** |
| Touche R | **Interrompt l'ordre actuel, maintient de la position et déselection de ou des unités** |

<a name="footnote1">*</a> Faire un clic gauche glissé n'empêche pas l'exécution de l'ordre potentiel de déplacement ou d'attaque, comme celle-ci n'est qu'exécutée sur la relâche du clic alors que les deux autres ordres sont exécutées dès le début du clic.

> Appuyer sur **CTRL** rend toute action de sélection additive.[**](#footnote2)

<a name="footnote2">**</a> Les unités déjà sélectionnées restent sélectionnées en plus des nouvelles.

## Copyright

Toutes les assets 3D et textures ont été récupérées depuis l'Unity Asset store, ou d'une source offrant un usage non limitant dans un cadre d'usage personnel, pédagogique et non lucratif.

La quasi-totalité des images 2D ont été faites par nous-même.