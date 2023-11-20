"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
require("dotenv/config");
const express_1 = __importDefault(require("express"));
const pg_1 = require("pg");
const bcrypt_1 = __importDefault(require("bcrypt"));
const jwt_1 = require("./jwt");
const db = new pg_1.Client({
    connectionString: process.env.POSTGRES_URI,
});
const app = (0, express_1.default)();
app.use(express_1.default.json());
app.post("/users/signup", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { username, password } = req.body;
    const hashedPassword = yield bcrypt_1.default.hash(password, 10);
    const query = `
    INSERT INTO users (username, password)
    SELECT $1::varchar, $2::varchar
    WHERE NOT EXISTS (
      SELECT 1 FROM users WHERE username = $1::varchar
    )
    RETURNING *;`;
    db.query(query, [username, hashedPassword])
        .then((queryRes) => __awaiter(void 0, void 0, void 0, function* () {
        if (queryRes.rowCount === 1) {
            const { id } = queryRes.rows[0];
            res.status(201).json({
                ID: id,
                AccessToken: yield (0, jwt_1.signAccessToken)(id, username),
            });
            return;
        }
        res.status(409).send("User already exists");
    }))
        .catch((err) => {
        console.log(err);
        res.status(500).send("Some unknown error happened");
    });
}));
app.post("/users/login", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { username, password } = req.body;
    const query = `
    SELECT (id, password, highscore, games_played)
    FROM users
    WHERE username = $1::varchar;
  `;
    const result = yield db.query(query, [username]).catch((err) => {
        console.error(err);
    });
    if (!result) {
        res.status(500).send("Something went wrong");
        return;
    }
    if (result.rows.length === 0) {
        res.status(401).send("Username not found");
        return;
    }
    let [dbId, dbPassword, dbHighscore, dbGamesPlayed] = result.rows[0].row
        .replace("(", "")
        .replace(")", "")
        .split(",");
    dbId = parseInt(dbId);
    dbHighscore = parseInt(dbHighscore);
    dbGamesPlayed = parseInt(dbGamesPlayed);
    const isCorrectPw = yield bcrypt_1.default
        .compare(password, dbPassword)
        .catch((err) => {
        console.error(err);
    });
    if (isCorrectPw) {
        res.status(200).json({
            ID: dbId,
            HighScore: dbHighscore,
            GamesPlayed: dbGamesPlayed,
            AccessToken: yield (0, jwt_1.signAccessToken)(dbId, username),
        });
        return;
    }
    res.status(401).send("Wrong password");
}));
app.put("/users/:id/highscore", jwt_1.verifyAccessToken, (req, res) => {
    const id = parseInt(req.params.id);
    const highscore = req.body.highscore;
    if (highscore === undefined || typeof highscore != "number") {
        res.status(400).send("Invalid highscore");
        return;
    }
    const query = `
    UPDATE users
    SET highscore = $1::int
    WHERE ID = $2::int;
  `;
    db.query(query, [highscore, id])
        .then((queryRes) => {
        res.status(200).send();
    })
        .catch((err) => {
        console.error(err);
        res.status(500).send("Something went wrong");
    });
});
app.put("/users/:id/inc-games-played", jwt_1.verifyAccessToken, (req, res) => {
    const id = parseInt(req.params.id);
    const gamesPlayed = req.body.games_played;
    if (gamesPlayed === undefined || typeof gamesPlayed != "number") {
        res.status(400).send("Invalid games_played");
        return;
    }
    const query = `
    UPDATE users
    SET games_played = games_played + 1
    WHERE ID = $1::int
    AND games_played = $2
  `;
    db.query(query, [id, gamesPlayed])
        .then((queryRes) => {
        res.status(200).send();
    })
        .catch((err) => {
        console.error(err);
        res.status(500).send("Something went wrong");
    });
});
const PORT = process.env.APP_PORT;
db.connect().then(() => {
    app.listen(PORT, () => {
        console.log(`Listening on port ${PORT}`);
    });
});
