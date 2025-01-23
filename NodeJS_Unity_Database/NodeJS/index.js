const express = require('express');
const usersRouter = require('./usersRouter');
const app = express();
const dbconfig = require('./database');
const body_parser = require('body-parser');

app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use('/users', usersRouter);

app.get('/', (req, res) =>{
    res.send('<h2>hello client</h2>');
});

app.listen(3000, () => {
    console.log('시작');
});