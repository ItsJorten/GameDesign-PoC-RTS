# Daily Git Checklist (Unity Project)

## ✅ Aan begin van de dag
1. Ga naar je projectmap in Git Bash:  
   ```bash
   cd pad/naar/jouw/unityproject
   ```
2. Check dat je op de juiste branch zit:  
   ```bash
   git branch
   ```
3. Update main (alleen als je remote/GitHub gebruikt):  
   ```bash
   git pull
   ```

---

## ✅ Tijdens het werken
- Regelmatig opslaan in Unity.
- Als je een logische stap af hebt (bijv. ResourceManager werkt):  
  ```bash
  git add .
  git commit -m "Beschrijving wat je gedaan hebt"
  ```
- Gebruik korte, duidelijke berichten (bijv. `Added ResourceManager + UI`).

---

## ✅ Nieuwe feature starten
```bash
git checkout main
git checkout -b feature/naam-van-feature
```

---

## ✅ Feature klaar? Terug naar main
```bash
git checkout main
git merge --no-ff feature/naam-van-feature -m "Merge: beschrijving"
git tag vX.X-feature
git push && git push --tags   # als je GitHub gebruikt
```

---

## ✅ Handige controles
- Status bekijken:  
  ```bash
  git status
  ```

- Compacte commitgeschiedenis:  
  ```bash
  git log --oneline --graph --decorate
  ```
