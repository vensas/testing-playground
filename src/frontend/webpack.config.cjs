const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const { parsed } = require('dotenv').config();

module.exports = (env) => {
  env = {
    ...parsed,
    ...env,
  };
  return {
    entry: './src/index.tsx', // entry point of the application
    output: {
      path: path.resolve(__dirname, 'dist'), // the bundle output path
      filename: 'bundle.js', // the name of the bundle
    },
    resolve: {
      extensions: ['.ts', '.tsx', '.js', '.jsx'], // file extensions to handle
      fallback: {
        process: require.resolve('process/browser'),
        buffer: require.resolve('buffer/'),
        crypto: require.resolve('crypto-browserify'),
        stream: require.resolve('stream-browserify'),
        os: require.resolve('os-browserify/browser'),
        path: require.resolve('path-browserify'),
        vm: require.resolve('vm-browserify'),
      },
    },
    plugins: [
      new HtmlWebpackPlugin({
        template: 'public/index.html', // to import index.html file inside index.js
      }),
      new webpack.ProvidePlugin({
        Buffer: ['buffer', 'Buffer'],
        process: 'process/browser',
      }),
      new webpack.DefinePlugin({
        'process.env.BACKEND_URL': JSON.stringify(env.BACKEND_URL),
      }),
    ],
    devServer: {
      port: 3000, // you can change the port
      historyApiFallback: true, // enable history API fallback for SPAs
    },
    module: {
      rules: [
        {
          test: /\.(ts|tsx)$/, // .ts and .tsx files
          exclude: /node_modules/, // excluding the node_modules folder
          use: {
            loader: 'babel-loader',
          },
        },
        {
          test: /\.(js|jsx)$/, // .js and .jsx files
          exclude: /node_modules/, // excluding the node_modules folder
          use: {
            loader: 'babel-loader',
          },
        },
        {
          test: /\.(sa|sc|c)ss$/, // styles files
          use: ['style-loader', 'css-loader', 'sass-loader'],
        },
        {
          test: /\.(png|woff|woff2|eot|ttf|svg)$/, // to import images and fonts
          loader: 'url-loader',
          options: { limit: false },
        },
      ],
    },
  };
};
