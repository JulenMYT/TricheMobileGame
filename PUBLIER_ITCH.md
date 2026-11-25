# Publier Imposteur sur itch.io

## 1. Construire le jeu

Dans Unity : menu **Build > WebGL pour itch.io**.

La première fois, Unity passe le projet sur la plateforme WebGL, ce qui peut prendre 10 à 20 minutes.
À la fin, l'explorateur s'ouvre sur `Builds/Imposteur-itch.zip`. C'est ce fichier qu'il faut envoyer.

Le script ([Assets/Editor/BuildItch.cs](Assets/Editor/BuildItch.cs)) applique automatiquement les bons réglages :

- compression Gzip avec « decompression fallback » : itch.io ne renvoie pas les en-têtes de compression, et sans ce réglage le jeu reste bloqué au chargement ;
- modèle de page **Imposteur** : format portrait centré, barre de chargement et bouton plein écran sur ordinateur ;
- le jeu continue de tourner quand la page perd le focus, ce qui est utile pour l'hôte d'une partie en ligne.

## 2. Créer la page sur itch.io

1. Sur itch.io : **Dashboard > Create new project**.
2. **Kind of project** : `HTML`.
3. **Uploads** : envoie `Imposteur-itch.zip`, puis coche **This file will be played in the browser**.
4. **Embed options** :
   - **Viewport dimensions** : `540` × `960` (format portrait) ;
   - coche **Mobile friendly**, avec l'orientation **Portrait** ;
   - coche **Fullscreen button** ;
   - laisse **SharedArrayBuffer support** décoché : il n'est pas nécessaire.
5. Enregistre, puis ouvre la page pour tester.

## 3. Mettre à jour le jeu

Refais la build depuis le menu, puis remplace le zip dans **Uploads** sur itch.io. Pas besoin de toucher aux autres réglages.

## Bon à savoir

- **Mode online** : il fonctionne dans le navigateur grâce aux WebSockets. Navigateurs, téléphones et PC peuvent jouer ensemble.
- **Tester à plusieurs sur un seul ordinateur** : utilise des navigateurs différents, ou une fenêtre de navigation privée. Deux onglets du même navigateur partagent le même compte anonyme et ne peuvent pas être dans le même salon.
- **Sauvegardes** : les options, les noms des joueurs et les catégories personnalisées sont stockés dans le navigateur. Vider les données du site les efface.
- **Hôte dans un onglet en arrière-plan** : les navigateurs ralentissent les onglets cachés. L'hôte doit garder l'onglet du jeu visible pendant une partie.
