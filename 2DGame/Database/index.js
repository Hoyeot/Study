const express = require('express');
const router = require('./router');
const cors = require('cors');
const app = express();
const dbconfig = require('./database');
const body_parser = require('body-parser');

app.use(express.json());
app.use(cors());
app.use(express.urlencoded({ extended: true }));
app.use('/users', router);

app.get('/', (req, res) =>{
    res.send('<h2>hello client</h2>');
});

app.listen(3000, () => {
    console.log('시작');
});