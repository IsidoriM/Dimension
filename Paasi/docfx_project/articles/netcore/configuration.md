<h1>Configurazione della libreria PASSI</h1>

Questa sezione spiega come effettuare la configurazione della libreria **PASSI** per il proprio servizio.

<h2>Come configurare il proprio servizio</h2>
Per configurare il servizio, basta definire nell'appsettings.json l'id del servizio assegnato dopo che questo è stato abilitato all'uso della libreria.

Rivolgersi al proprio referente INPS per far abilitare il proprio servizio.

Una volta abilitato, basterà aggiungere la seguente configurazione nel proprio *appsettings.json*
```json
"ServiceId": 1 //Qui va l'id del tuo servizio 
```
È possibile specificare anche dei parametri opzionali nell'*appsettings.json*.<br/>
Di seguito la lista dei parametri opzionali inizializzata con i valori di default:

```json
"GestioneSessione": "1",
"Log": "0"
```

La variabile Log può assumere i seguenti valori:
```
Trace = 0,
Debug = 1,
Information = 2,
Warning = 3,
Error = 4,
Critical = 5,
None = 6
```

La variabile GestioneSessione può assumere i seguenti valori:
```
SessioneUnica = 1,
SessioneMultipla = 3
```