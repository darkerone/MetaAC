# MetaAC

## Problématique
Les musiques téléchargée sur internet (via Youtube par exemple) ne contiennent pas de métadonnées (titre, artiste, image,...).

## Solution
MetaAC permet d'ajouter des métadonnées aux musiques de manière simple et rapide.
Les métadonnées ajoutées sont :
- Titre
- Artiste
- Album
- Année
- Image

L'outil offre plusieurs manières d'ajouter les métadonnées.
- Automatiquement : la recherche est effectuée via plusieurs API dans l'ordre suivant :
  - AcoustID : créé une empreinte numérique de la musique et la compare à la base de données de AcoustID (https://acoustid.org/). 
    Effectue ensuite une recherche sur les API suivante pour obtenir plus d'informations.
  - Itunes : utilise le nom de l'artiste et le titre de la musique trouvés dans le nom du fichier ("Artiste - Titre")
  - MusixMatch : utilise le nom de l'artiste et le titre de la musique trouvés dans le nom du fichier ("Artiste - Titre")
- Semi-Automatiquement : la recherche est effectuée en utilisant le nom d'artiste et le titre fournis par l'utilisateur sur les API Itunes et MusixMatch
- Manuellement : les données sont entrées manuellement par l'utilisateur
