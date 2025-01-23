const mysql = require('mysql2');

const pool = mysql.createPool({
    host: 'localhost',
    port: 'port',
    user: 'root',
    password: 'root',
    database: 'db_name'
});

const getUsers = async() => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query('select * From user_mst;');
    console.log(rows);
    return rows;
};

const insertUser = async (values) => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query('Insert into user_mst (user_id, user_pw, user_name, user_nickname) values (?, ?, ?, ?);', values);
    return rows;
}

const deleteUser = async (values) => {
    const promisePool = pool.promise();
        const [rows] = await promisePool.query('delete from user_mst where user_id = ?;', [userId]);
    return rows;
}

const UpdateUser = async (values) => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query('update user_mst set high_point = ? where user_id = ?;', [updatedValues, userId]);
    return rows;
}

module.exports = {
    getUsers, insertUser, deleteUser, UpdateUser
};
