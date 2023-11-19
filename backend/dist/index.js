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
    );`;
    db.query(query, [username, hashedPassword])
        .then((queryRes) => {
        if (queryRes.rowCount === 1) {
            res.status(201).send();
            return;
        }
        res.status(401).send("User already exists");
    })
        .catch((err) => {
        console.log(err);
        res.status(500).send("Some unknown error happened");
    });
}));
app.post("/users/login", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { username, password } = req.body;
    const query = `
    SELECT (password, highscore, games_played)
    FROM users
    WHERE username = $1::varchar;
  `;
    const result = yield db.query(query, [username]);
    if (result.rows.length === 0) {
        res.status(401).send("Username not found");
        return;
    }
    let [dbPassword, dbHighscore, dbGamesPlayed] = result.rows[0].row
        .replace("(", "")
        .replace(")", "")
        .split(",");
    dbHighscore = parseInt(dbHighscore);
    dbGamesPlayed = parseInt(dbGamesPlayed);
    const isCorrectPw = yield bcrypt_1.default.compare(password, dbPassword);
    if (isCorrectPw) {
        res.status(200).send({
            highscore: dbHighscore,
            gamesPlayed: dbGamesPlayed,
        });
        return;
    }
    res.status(401).send();
}));
const PORT = process.env.APP_PORT;
db.connect().then(() => {
    app.listen(PORT, () => {
        console.log(`Listening on port ${PORT}`);
    });
});
